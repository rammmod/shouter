using Rhinero.Shouter.Shared;
using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.App
{
    internal class RabbitMQ
    {
        [Required]
        public string Hostname { get; set; }

        public ushort Port { get; set; } = Constants.DefaultRabbitMQ.Port;

        public string VirtualHost { get; set; } = Constants.DefaultRabbitMQ.VirtualHost;

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PublishQueue { get; set; } = Constants.DefaultRabbitMQ.PublishQueue;

        public string ReplyQueue { get; set; } = Constants.DefaultRabbitMQ.ReplyQueue;

        public int PrefetchCount { get; set; } = Constants.DefaultRabbitMQ.PrefetchCount;

        public int ConcurrentMessageLimit { get; set; } = Constants.DefaultRabbitMQ.ConcurrentMessageLimit;
    }
}
