namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class FileNameOrClientNameObligatoryException : Exception
    {
        public FileNameOrClientNameObligatoryException() : base($"One of file name or client name is obligatory") { }
    }
}
