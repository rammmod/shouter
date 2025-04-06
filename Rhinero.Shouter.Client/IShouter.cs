using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Contracts.Enums;

namespace Rhinero.Shouter.Client
{
    public interface IShouter
    {
        Task<Guid> ShoutAsync(Buses bus, Protocol protocol, object payload, CancellationToken cancellationToken = default);
        Guid Shout(Buses bus, Protocol protocol, object payload);
    }
}
