namespace Rhinero.Shouter.Shared.Exceptions
{
    public sealed class RabbitMQPublishException : Exception
    {
        public RabbitMQPublishException() : base("RabbitMQ publish failed") { }
    }
}