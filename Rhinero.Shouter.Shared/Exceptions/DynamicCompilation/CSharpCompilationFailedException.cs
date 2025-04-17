namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class CSharpCompilationFailedException : Exception
    {
        public CSharpCompilationFailedException(string message) : base($"C# Compilation failed: {message}") { }
    }
}
