using System.Diagnostics;

namespace Rhinero.Shouter.Shared.Exceptions.DynamicCompilation
{
    public sealed class CSharpFileGenerationException : Exception
    {
        public CSharpFileGenerationException(string message) : base($"Error generating C# files: {message}") { }
    }
}
