namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class ShouterInterfaceImplementationException : Exception
    {
        public ShouterInterfaceImplementationException() : base("Payload should implement IShouterPayload interface") { }
    }
}