namespace Rhinero.Shouter.Shared.Exceptions
{
    public sealed class ShouterBusConfigurationException : Exception
    {
        public ShouterBusConfigurationException() : base("One of bus configurations should be set")
        {
        }
    }
}