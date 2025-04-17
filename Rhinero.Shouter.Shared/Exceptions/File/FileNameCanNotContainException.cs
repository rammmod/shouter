namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileNameCanNotContainException : Exception
    {
        public FileNameCanNotContainException(string message) : base($"File name can not contain {message}") { }
    }
}
