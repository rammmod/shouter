using Rhinero.Shouter.Shared;
using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.App
{
    public class Kafka
    {
        [Required]
        public string BootstrapServers { get; set; }

        public string PublishTopic { get; set; } = Constants.DefaultKafka.PublishTopic;

        public string PublishGroup { get; set; } = Constants.DefaultKafka.PublishGroup;

        public string RequestTopic { get; set; } = Constants.DefaultKafka.RequestTopic;

        public string RequestGroup { get; set; } = Constants.DefaultKafka.RequestGroup;

        public string ReplyTopic { get; set; } = Constants.DefaultKafka.ReplyTopic;
    }
}
