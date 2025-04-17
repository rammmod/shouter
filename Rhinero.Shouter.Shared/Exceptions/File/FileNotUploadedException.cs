namespace Rhinero.Shouter.Shared.Exceptions.File
{
    public sealed class FileNotUploadedException : Exception
    {
        public FileNotUploadedException() : base($"File is not uploaded") { }
    }
}
