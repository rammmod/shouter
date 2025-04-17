namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class HttpBodyAndContentTypeInconsistencyException : Exception
    {
        public HttpBodyAndContentTypeInconsistencyException() : base("Http body and content type are inconsistent") { }
    }
}