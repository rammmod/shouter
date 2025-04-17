using Microsoft.AspNetCore.Http;

namespace Rhinero.Shouter.Shared.Extensions
{
    internal static class FileFormExtensions
    {
        internal static bool IsNullOrEmpty(this IFormFile file) =>
            file is null || file.Length is 0;
    }
}
