namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class KafkaProduceException : Exception
    {
        public KafkaProduceException() : base("Kafka produce failed") { }
    }
}