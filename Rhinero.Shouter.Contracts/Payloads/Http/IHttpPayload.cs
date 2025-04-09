namespace Rhinero.Shouter.Contracts.Payloads.Http
{
    internal interface IHttpPayload
    {
        public string Body { get; init; }
        public ContentTypeEnum ContentType { get; init; }
    }
}
