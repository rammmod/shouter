using System.Net;

namespace Rhinero.Shouter.Contracts.Payloads.Http
{
    public class HttpPayload : IShouterPayload
    {
        public Uri Uri { get; init; }
        public HttpMethod Method { get; init; }
        public Dictionary<string, string> Headers { get; init; }
        public Dictionary<string, string> QueryParameters { get; init; }
        public string Body { get; init; }
        public string ContentType { get; init; }
        public string Token { get; init; }
        public NetworkCredential Credentials { get; init; }

        //TODO: add missing parameters
    }
}
