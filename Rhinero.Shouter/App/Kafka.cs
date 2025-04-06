using Rhinero.Shouter.Shared;
using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.App
{
    public class Kafka
    {
        [Required]
        public string BootstrapServers { get; set; }

        public string Topic { get; set; } = Constants.DefaultKafka.Topic;

        public string Group { get; set; } = Constants.DefaultKafka.Group;
    }
}
