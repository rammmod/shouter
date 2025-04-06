using Rhinero.Shouter.Contracts;

namespace Rhinero.Shouter.Interfaces
{
    public interface ICallbackService
    {
        Task SendAsync(ShouterMessage message);
    }
}
