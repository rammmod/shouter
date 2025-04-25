namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class RequestMethodNotFoundInAssemblyException : Exception
    {
        public RequestMethodNotFoundInAssemblyException(string message) : base($"Request method {message} is not found in the assembly") { }
    }
}
