namespace Rhinero.Shouter.Shared.Exceptions
{
    public sealed class KafkaProduceException : Exception
    {
        public KafkaProduceException() : base("Kafka produce failed") { }
    }
}