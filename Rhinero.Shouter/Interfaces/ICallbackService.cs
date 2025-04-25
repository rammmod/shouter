using Rhinero.Shouter.Contracts;

namespace Rhinero.Shouter.Interfaces
{
    public interface ICallbackService
    {
        Task SendAsync(ShouterMessage message, CancellationToken cancellationToken);
        Task<object> ReplyAsync(ShouterRequestMessage message, CancellationToken cancellationToken);
    }
}
