namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class ClientTypeInAssemblyNotFoundException : Exception
    {
        public ClientTypeInAssemblyNotFoundException(string message) : base($"Client type {message} is not found in the assembly") { }
    }
}
