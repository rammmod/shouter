namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class GrpcCSharpPluginNotFoundException : Exception
    {
        public GrpcCSharpPluginNotFoundException(string path) : base($"gRPC CSharp plugin is not found in directory {path}") { }
    }
}
