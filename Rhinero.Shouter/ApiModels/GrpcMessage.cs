using System.ComponentModel.DataAnnotations;

namespace Rhinero.Shouter.ApiModels
{
    public class GrpcMessage
    {
        [Required]
        [RegularExpression("^[A-Z][\\w\\-]*\\.proto$")]
        public string Name { get; set; }

        [Required]
        public string FileContent { get; set; }
    }
}
