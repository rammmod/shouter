using System.Net;

namespace Rhinero.Shouter.Client
{
    public interface IShouter
    {
        Task<Guid> ShoutAsync(string uri, object payload, string token = null, NetworkCredential credentials = null, CancellationToken cancellationToken = default);
        Task<Guid> ShoutAsync(Uri uri, object payload, string token = null, NetworkCredential credentials = null, CancellationToken cancellationToken = default);
        Task<Guid> ShoutAsync(Uri uri, ShouterMethodEnums method, string payload, CancellationToken cancellationToken = default, string contentType = "application/json", string token = null, NetworkCredential credentials = null);
        Guid Shout(Uri uri, ShouterMethodEnums method, string payload, CancellationToken cancellationToken = default, string contentType = "application/json", string token = null, NetworkCredential credentials = null);
    }
}
