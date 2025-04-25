namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class GrpcAssemblyNotFoundException : Exception
    {
        public GrpcAssemblyNotFoundException(string message) : base($"Grpc assembly is not found: {message}") { }
    }
}
