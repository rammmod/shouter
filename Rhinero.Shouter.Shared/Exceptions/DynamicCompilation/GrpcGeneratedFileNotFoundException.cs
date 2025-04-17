namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class GrpcGeneratedFileNotFoundException : Exception
    {
        public GrpcGeneratedFileNotFoundException(string fileName) : base($"gRPC CSharp generated file is not found: {fileName}") { }
    }
}
