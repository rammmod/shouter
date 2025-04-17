namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileExtensionException : Exception
    {
        public FileExtensionException(string message) : base($"Only {message} files are allowed") { }
    }
}
