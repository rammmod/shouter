using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.App
{
    public class Protoc
    {
        [Required]
        public string Path { get; set; }
    }
}
