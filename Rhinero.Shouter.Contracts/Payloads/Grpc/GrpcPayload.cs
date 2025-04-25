using Rhinero.Shouter.Shared;
using Rhinero.Shouter.Shared.Exceptions.File;

namespace Rhinero.Shouter.Contracts.Payloads.Grpc
{
    public sealed class GrpcPayload : IShouterPayload
    {
        public Uri Uri { get; init; }
        public GrpcService Service { get; set; }
        public string RequestMethod { get; init; }
        public string RequestArgumentName { get; init; }
        public Dictionary<string, string> RequestParameters { get; init; }
        public Dictionary<string, string> RequestMetadata { get; init; }
        public int? RequestDeadlineInSeconds { get; init; }

        public string ResponseArgumentName { get; init; }
    }

    public sealed class GrpcService
    {
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            init
            {
                if (value.Contains(Constants.FileExtensions.Proto, StringComparison.InvariantCultureIgnoreCase))
                    throw new FileNameCanNotContainException(Constants.FileExtensions.Proto);

                _fileName = value;
            }
        }
        public string ClientName { get; init; }
    }
}
