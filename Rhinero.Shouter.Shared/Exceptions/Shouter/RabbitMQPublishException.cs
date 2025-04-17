namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class RabbitMQPublishException : Exception
    {
        public RabbitMQPublishException() : base("RabbitMQ publish failed") { }
    }
}