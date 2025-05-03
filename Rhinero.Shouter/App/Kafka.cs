using Confluent.Kafka;
using Rhinero.Shouter.Shared;
using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.App
{
    internal class Kafka
    {
        [Required]
        public string BootstrapServers { get; set; }

        public string PublishTopic { get; set; } = Constants.DefaultKafka.PublishTopic;
        public string PublishGroup { get; set; } = Constants.DefaultKafka.PublishGroup;
        public string RequestTopic { get; set; } = Constants.DefaultKafka.RequestTopic;
        public string RequestGroup { get; set; } = Constants.DefaultKafka.RequestGroup;
        public string ReplyTopic { get; set; } = Constants.DefaultKafka.ReplyTopic;

        public int? PrefetchCount { get; set; } = 16;
        public int? ConcurrentMessageLimit { get; set; } = 16;
        public int? ConcurrentDeliveryLimit { get; set; } = 16;
        public ushort? ConcurrentConsumerLimit { get; set; } = 16;

        public Sasl Sasl { get; set; }
        public Ssl Ssl { get; set; }
    }

    internal class Sasl
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public SaslMechanism Mechanism { get; set; }
    }

    internal class Ssl
    {
        public bool EnableSslCertificateVerification { get; set; } = false;

        public bool UseClientCertificate { get; set; } = false;

        public string CaCertificateLocation { get; set; }
        public string ClientCertificateLocation { get; set; }
        public string KeyLocation { get; set; }
        public string KeyPassword { get; set; }
        public string KeystoreLocation { get; set; }
        public string KeystorePassword { get; set; }
    }
}
