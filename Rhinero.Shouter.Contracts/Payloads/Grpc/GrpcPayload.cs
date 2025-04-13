namespace Rhinero.Shouter.Contracts.Payloads.Grpc
{
    public class GrpcPayload : IShouterPayload
    {
        private string _fileName;

        public Uri Uri { get; init; }
        public string FileName
        {
            get => _fileName.EndsWith(".proto", StringComparison.InvariantCulture) ? _fileName : string.Concat(_fileName, ".proto");
            init => _fileName = value;
        }
        public string Request { get; init; }
    }
}
