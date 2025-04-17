using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Reflection;

namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var protoPath = @"protos\testproto.proto";
            var outputDir = @"protos\generated";
            var generatedFile = Path.Combine(outputDir, "TestprotoGrpc.cs");

            RunProtoc(protoPath);

            if (!File.Exists(generatedFile))
            {
                Console.WriteLine("❌ Generated file not found: " + generatedFile);
                return;
            }

            var syntaxTrees = Directory.GetFiles("protos/generated", "*.cs")
                .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file)))
                .ToList();

            var loadedAssemblies = new List<MetadataReference>();

            void AddRef(Type type)
            {
                var location = type.Assembly.Location;
                loadedAssemblies.Add(MetadataReference.CreateFromFile(location));
            }

            AddRef(typeof(object));                        // System.Private.CoreLib
            AddRef(typeof(Console));                       // System.Console
            AddRef(typeof(System.Net.Http.HttpClient));    // System.Net.Http
            AddRef(typeof(Grpc.Net.Client.GrpcChannel));   // Grpc.Net.Client
            AddRef(typeof(Grpc.Core.CallInvoker));         // Grpc.Core
            AddRef(typeof(Google.Protobuf.IMessage));      // Google.Protobuf

            loadedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location)));

            var compilation = CSharpCompilation.Create("DynamicGrpcAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(loadedAssemblies)
                .AddSyntaxTrees(syntaxTrees);

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                Console.WriteLine("Compilation failed!");
                foreach (var diag in emitResult.Diagnostics)
                    Console.WriteLine(diag);
                return;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            var types = assembly.GetTypes();











            // 1. Find the generated client class (e.g. Greeter.GreeterClient)
            var greeterClientType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.EndsWith("Client") && t.BaseType?.Name.Contains("ClientBase") == true);

            if (greeterClientType == null)
            {
                Console.WriteLine("❌ gRPC client class not found.");
                return;
            }

            Console.WriteLine($"✅ Found client class: {greeterClientType.FullName}");

            var helloRequestType = assembly.GetTypes().First(t => t.Name.EndsWith("HelloRequest"));
            var helloReplyType = assembly.GetTypes().First(t => t.Name.EndsWith("HelloReply"));
            //var greeterClientType = assembly.GetTypes().First(t => t.Name.EndsWith("GreeterClient"));

            // Create instance of GreeterClient using channel
            using var channel = GrpcChannel.ForAddress("http://localhost:5215"); // update to your server
            var ctor = greeterClientType.GetConstructor(new[] { typeof(Grpc.Core.CallInvoker) });
            var greeterClient = ctor.Invoke(new object[] { channel.CreateCallInvoker() });

            // Build HelloRequest message
            var helloRequest = Activator.CreateInstance(helloRequestType);
            helloRequestType.GetProperty("Name")?.SetValue(helloRequest, "DynamicUser");

            // Get exact method with 4 parameters
            var sayHelloMethod = greeterClientType.GetMethod("SayHello", new[]
            {
                helloRequestType,
                typeof(Grpc.Core.Metadata),
                typeof(DateTime?),
                typeof(CancellationToken)
            });

            if (sayHelloMethod == null)
            {
                Console.WriteLine("❌ Could not find SayHello(HelloRequest, Metadata, DateTime?, CancellationToken)");
                return;
            }

            // Call the method
            var metadata = new Grpc.Core.Metadata(); // Add any headers if needed
            DateTime? deadline = null;     // Or set deadline e.g., DateTime.UtcNow.AddSeconds(10)
            var cancellationToken = CancellationToken.None;

            var reply = sayHelloMethod.Invoke(greeterClient, new object[]
            {
                helloRequest,
                metadata,
                deadline,
                cancellationToken
            });

            // Read response
            var message = helloReplyType.GetProperty("Message")?.GetValue(reply);
            Console.WriteLine($"✅ Server responded: {message}");
        }

        public static void RunProtoc(string protoFilePath)
        {
            var protocPath = @"protos\protoc.exe";  // Specify the path to protoc
            var outputDir = @"protos\generated";   // Specify the output directory for generated files

            var pluginPath = FindPlugin();

            var arguments = $"-I={Path.GetDirectoryName(protoFilePath)} --csharp_out=\"{outputDir}\" --grpc_out={outputDir} --plugin=protoc-gen-grpc={pluginPath} {protoFilePath}";

            var startInfo = new ProcessStartInfo
            {
                FileName = protocPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    Console.WriteLine($"Error generating C# files: {error}");
                }
            }

            string? FindPlugin()
            {
                var nugetRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
                var toolsPath = Path.Combine(nugetRoot, "grpc.tools");

                if (!Directory.Exists(toolsPath)) return null;

                var exe = Directory.GetFiles(toolsPath, "grpc_csharp_plugin.exe", SearchOption.AllDirectories)
                                   .FirstOrDefault();
                return exe;
            }
        }
    }
}
