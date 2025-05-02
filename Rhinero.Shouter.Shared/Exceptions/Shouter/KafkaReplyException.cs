namespace Rhinero.Shouter.Shared.Exceptions.Shouter
{
    public sealed class KafkaReplyException : Exception
    {
        public KafkaReplyException() : base("Kafka reply failed") { }
    }
}