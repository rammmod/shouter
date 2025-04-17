using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Exceptions.File;
using Rhinero.Shouter.Shared.Extensions;

namespace Rhinero.Shouter.Services.Protos
{
    public class ProtoRegistration : IProtoRegistration
    {
        private static string[] _allowedContentTypes = [Constants.FileContentTypes.TextPlainContentType, Constants.FileContentTypes.OctetStreamContentType];

        private readonly IProtoCache _protoCache;

        public ProtoRegistration(IProtoCache protoCache)
        {
            _protoCache = protoCache;
        }

        public async Task<KeyValuePair<string, string>> GetAsync(string fileName, CancellationToken cancellationToken) =>
            await _protoCache.GetAsync(fileName, cancellationToken);

        public async Task<ICollection<string>> GetAllFileNamesAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(_protoCache.GetAllKeys());
        }

        public async Task AddAsync(IFormFile file, CancellationToken cancellationToken)
        {
            CheckFileCorrectness(file);

            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync(cancellationToken);
            }

            await _protoCache.AddAsync(file.FileName, content, cancellationToken);
        }

        public async Task PutAsync(IFormFile file, CancellationToken cancellationToken)
        {
            CheckFileCorrectness(file);

            string content;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                content = await reader.ReadToEndAsync(cancellationToken);
            }

            await _protoCache.UpdateAsync(file.FileName, content, cancellationToken);
        }

        public async Task DeleteAsync(string fileName, CancellationToken cancellationToken)
        {
            _protoCache.DeleteAsync(fileName);
            await Task.CompletedTask;
        }

        private static void CheckFileCorrectness(IFormFile file)
        {
            if (file.IsNullOrEmpty())
                throw new FileNotUploadedException();

            if (Path.GetExtension(file.FileName).ToLowerInvariant() is not Constants.FileExtensions.Proto)
                throw new FileExtensionException(Constants.FileExtensions.Proto);

            if (file.FileName.ToLowerInvariant().Remove(file.FileName.Length - Constants.FileExtensions.Proto.Length).Contains(Constants.FileExtensions.Proto))
                throw new FileNameCanNotContainException(Constants.FileExtensions.Proto);

            if (!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new FileContentTypeException(file.ContentType);
        }
    }
}
