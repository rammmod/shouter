namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class RabbitMQReplyException : Exception
    {
        public RabbitMQReplyException() : base("RabbitMQ reply failed") { }
    }
}