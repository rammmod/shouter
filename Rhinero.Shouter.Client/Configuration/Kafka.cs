using Rhinero.Shouter.Shared;
using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.Client.Configuration
{
    internal class Kafka
    {
        [Required]
        public string BootstrapServers { get; set; }

        public string PublishTopic { get; set; } = Constants.DefaultKafka.PublishTopic;
        public string RequestTopic { get; set; } = Constants.DefaultKafka.RequestTopic;
        public string ReplyTopic { get; set; } = Constants.DefaultKafka.ReplyTopic;
        public string ReplyGroup { get; set; } = Constants.DefaultKafka.ReplyGroup;

        [Required]
        public Redis Redis { get; set; }
    }
}
