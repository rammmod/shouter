using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rhinero.Shouter.App;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Exceptions.DynamicCompilation;
using Rhinero.Shouter.Shared.Exceptions.File;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Rhinero.Shouter.Services.Protos
{
    public class ProtoCache : IProtoCache
    {
        private readonly Protoc _protoc;
        private readonly ICollection<MetadataReference> _metadataReferences;

        private readonly Dictionary<string, Assembly> _cache = [];

        public ProtoCache(Protoc protoc)
        {
            _protoc = protoc;
            _metadataReferences = LoadAssembliesMetadata();

            if (!Directory.Exists(Constants.Directories.ProtoFiles))
                Directory.CreateDirectory(Constants.Directories.ProtoFiles);

            if (!Directory.Exists(Constants.Directories.ProtoFilesGenerated))
                Directory.CreateDirectory(Constants.Directories.ProtoFilesGenerated);

            Build();
        }

        private void Build()
        {
            GenerateNewProtos();

            var groupedGeneratedFiles = GetGroupedGeneratedFiles();

            foreach (var item in groupedGeneratedFiles)
                _cache.Add(item, GenerateAssembly(item, _metadataReferences));
        }

        #region Cache region

        public Assembly GetAssemblyByKey(string key)
        {
            if (_cache.TryGetValue(key, out var assembly))
                return assembly;

            throw new GrpcAssemblyNotFoundException(key);
        }

        public Assembly GetAssemblyByClient(string clientName)
        {
            return _cache.Values.Where(a => a.GetTypes().FirstOrDefault(t =>
                    t.Name.Equals(clientName + Constants.Assembly.Client) is true &&
                    t.BaseType?.Name.Equals(clientName + Constants.Assembly.ClientBase) is true) is not null).FirstOrDefault() ??
            throw new GrpcAssemblyNotFoundException(clientName);
        }

        #endregion

        #region Api region

        public async Task<KeyValuePair<string, string>> GetAsync(string key, CancellationToken cancellationToken)
        {
            if (!_cache.ContainsKey(key))
                return new KeyValuePair<string, string>();

            var fileContent = await File.ReadAllTextAsync(
                Path.Combine(Constants.Directories.ProtoFiles, key + Constants.FileExtensions.Proto),
                cancellationToken);

            return new KeyValuePair<string, string>(key, fileContent);
        }

        public ICollection<string> GetAllKeys() =>
            _cache.Keys;

        public async Task AddAsync(string fileName, string content, CancellationToken cancellationToken)
        {
            var protoFilePath = Path.Combine(Constants.Directories.ProtoFiles, fileName);

            try
            {
                if (File.Exists(protoFilePath))
                    throw new FileExistsException(fileName);

                await File.WriteAllTextAsync(protoFilePath, content, cancellationToken);

                RunProtoc(protoFilePath, FindGrpcCSharpPlugin());

                var protoFileName = fileName.Replace(Constants.FileExtensions.Proto, string.Empty);

                _cache.Add(protoFileName, GenerateAssembly(protoFileName, _metadataReferences));
            }
            catch
            {
                File.Delete(protoFilePath);
                throw;
            }
        }

        public async Task UpdateAsync(string fileName, string content, CancellationToken cancellationToken)
        {
            var protoFilePath = Path.Combine(Constants.Directories.ProtoFiles, fileName);

            if (!File.Exists(protoFilePath))
                throw new FileNotExistException(fileName);

            var originalContent = await File.ReadAllTextAsync(protoFilePath, cancellationToken);

            try
            {
                if (content.Equals(originalContent, StringComparison.Ordinal))
                    return;

                await File.WriteAllTextAsync(protoFilePath, content, cancellationToken);

                RunProtoc(protoFilePath, FindGrpcCSharpPlugin());

                var protoFileName = fileName.Replace(Constants.FileExtensions.Proto, string.Empty);

                _cache.Remove(protoFileName);
                _cache.Add(protoFileName, GenerateAssembly(protoFileName, _metadataReferences));
            }
            catch
            {
                await File.WriteAllTextAsync(protoFilePath, originalContent, cancellationToken);
                throw;
            }
        }

        public void DeleteAsync(string fileName)
        {
            var protoFilePath = Path.Combine(Constants.Directories.ProtoFiles, fileName + Constants.FileExtensions.Proto);
            var generatedCsFilePath = Path.Combine(Constants.Directories.ProtoFilesGenerated, fileName + Constants.FileExtensions.Cs);
            var generatedGrpcCsFilePath = Path.Combine(
                Constants.Directories.ProtoFilesGenerated, fileName + Constants.FileExtensions.GrpcName + Constants.FileExtensions.Cs);

            if (!File.Exists(protoFilePath) && !File.Exists(generatedCsFilePath) && !File.Exists(generatedGrpcCsFilePath))
                throw new FileNotExistException(fileName);

            if (File.Exists(protoFilePath))
                File.Delete(protoFilePath);

            if (File.Exists(generatedCsFilePath))
                File.Delete(generatedCsFilePath);

            if (File.Exists(generatedGrpcCsFilePath))
                File.Delete(generatedGrpcCsFilePath);

            _cache.Remove(fileName);
        }

        #endregion

        #region helpers

        private static Assembly GenerateAssembly(string item, ICollection<MetadataReference> loadedAssemblies)
        {
            var generatedCs = item + Constants.FileExtensions.Cs;
            var generatedGrpcCs =
                item.Replace(Constants.FileExtensions.Proto, string.Empty) + Constants.FileExtensions.GrpcName + Constants.FileExtensions.Cs;

            if (!File.Exists(Path.Combine(Constants.Directories.ProtoFilesGenerated, generatedCs)) ||
                !File.Exists(Path.Combine(Constants.Directories.ProtoFilesGenerated, generatedGrpcCs)))
                throw new GrpcGeneratedFileNotFoundException(generatedGrpcCs);

            var syntaxTrees = Directory.GetFiles(Constants.Directories.ProtoFilesGenerated, Constants.FileExtensions.WildcardCs)
                .Where(file => Path.GetFileName(file) == generatedCs || Path.GetFileName(file) == generatedGrpcCs)
                .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file)))
                .ToList();

            var compilation = CSharpCompilation.Create(string.Concat(Constants.Assembly.DynamicGrpcAssembly, item))
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(loadedAssemblies)
                .AddSyntaxTrees(syntaxTrees);

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
                throw new CSharpCompilationFailedException(string.Join(Environment.NewLine, emitResult.Diagnostics));

            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        private void GenerateNewProtos()
        {
            var protoFiles = Directory.GetFiles(Constants.Directories.ProtoFiles, Constants.FileExtensions.WildcardProto);

            var groupedGeneratedFiles = GetGroupedGeneratedFiles();

            var nonGeneratedFiles = protoFiles.Where(x =>
                !groupedGeneratedFiles.Contains(Path.GetFileNameWithoutExtension(x)))
                .ToList();

            var pluginPath = FindGrpcCSharpPlugin();

            foreach (var item in nonGeneratedFiles)
                RunProtoc(item, pluginPath);
        }

        private static HashSet<string> GetGroupedGeneratedFiles()
        {
            var generatedFiles = Directory.GetFiles(Constants.Directories.ProtoFilesGenerated, Constants.FileExtensions.WildcardCs);

            return generatedFiles
                .GroupBy(f =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    return fileName.EndsWith(Constants.FileExtensions.GrpcName) ? fileName[..^4] : fileName;
                })
                .Where(g => g.Count() is 2)
                .Select(g => g.Key)
                .ToHashSet();
        }

        private void RunProtoc(string protoFilePath, string pluginPath)
        {
            var arguments = $"-I={Path.GetDirectoryName(protoFilePath)} --csharp_out=\"{Constants.Directories.ProtoFilesGenerated}\" --grpc_out={Constants.Directories.ProtoFilesGenerated} --plugin=protoc-gen-grpc={pluginPath} {protoFilePath}";

            var startInfo = new ProcessStartInfo
            {
                FileName = _protoc.Path,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new CSharpFileGenerationException(process.StandardError.ReadToEnd());
        }

        private static string FindGrpcCSharpPlugin()
        {
            var nugetRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            var toolsPath = Path.Combine(nugetRoot, "grpc.tools");

            if (!Directory.Exists(toolsPath))
                throw new DirectoryNotFoundException(toolsPath);

            return Directory.GetFiles(toolsPath, GetPluginName(), SearchOption.AllDirectories).FirstOrDefault() ??
                throw new GrpcCSharpPluginNotFoundException(toolsPath);

            static string GetPluginName()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return "grpc_csharp_plugin.exe";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "grpc_csharp_plugin";
                else
                    throw new PlatformNotSupportedException();
            }
        }

        private static ICollection<MetadataReference> LoadAssembliesMetadata()
        {
            var loadedAssemblies = new List<MetadataReference>();

            void AddMetadataReference(Type type)
            {
                var location = type.Assembly.Location;
                loadedAssemblies.Add(MetadataReference.CreateFromFile(location));
            }

            AddMetadataReference(typeof(object));                        // System.Private.CoreLib
            AddMetadataReference(typeof(Console));                       // System.Console
            AddMetadataReference(typeof(System.Net.Http.HttpClient));    // System.Net.Http
            AddMetadataReference(typeof(Grpc.Net.Client.GrpcChannel));   // Grpc.Net.Client
            AddMetadataReference(typeof(Grpc.Core.CallInvoker));         // Grpc.Core
            AddMetadataReference(typeof(Google.Protobuf.IMessage));      // Google.Protobuf

            loadedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location)));

            return loadedAssemblies;
        }

        #endregion
    }
}
