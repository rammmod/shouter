namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileExistsException : Exception
    {
        public FileExistsException(string message) : base($"Proto file with {message} already exists") { }
    }
}
