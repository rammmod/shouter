using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.Client.Configuration
{
    internal class Redis
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public int CacheId { get; set; }
    }
}
