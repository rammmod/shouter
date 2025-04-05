using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.Client.Configuration
{
    internal class RabbitMQ
    {
        [Required]
        public string Hostname { get; set; }

        public ushort Port { get; set; } = 5672;

        public string VirtualHost { get; set; } = "/";

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public string QueueName { get; set; } = "ShouterEvent";
    }
}
