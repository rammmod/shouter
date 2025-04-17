namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileNotExistException : Exception
    {
        public FileNotExistException(string message) : base($"Proto file with {message} does not exist") { }
    }
}
