namespace Rhinero.Shouter.Shared.Exceptions
{
    public sealed class HttpBodyAndContentTypeInconsistencyException : Exception
    {
        public HttpBodyAndContentTypeInconsistencyException() : base("Http body and content type are inconsistent")
        {
        }
    }
}