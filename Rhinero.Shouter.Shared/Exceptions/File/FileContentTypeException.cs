namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileContentTypeException : Exception
    {
        public FileContentTypeException(string message) : base($"Content type {message} is not supported. Only text/plain or application/octet-stream content types are allowed") { }
    }
}
