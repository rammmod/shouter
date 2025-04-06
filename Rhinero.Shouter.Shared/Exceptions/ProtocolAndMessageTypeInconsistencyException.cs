namespace Rhinero.Shouter.Shared.Exceptions
{
    public sealed class ProtocolAndMessageTypeInconsistencyException : Exception
    {
        public ProtocolAndMessageTypeInconsistencyException() : base("Protocol and message type are inconsistent")
        {
        }
    }
}