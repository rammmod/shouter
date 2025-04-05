namespace Rhinero.Shouter.App
{
    public class MassTransitConfiguration
    {
        public string Hostname { get; set; }
        public ushort? Port { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public int? PrefetchCount { get; set; }
        public int? ConcurrentMessageLimit { get; set; }
    }
}
