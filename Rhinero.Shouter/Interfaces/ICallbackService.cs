using Rhinero.Shouter.Shared.Contracts;

namespace Rhinero.Shouter.Interfaces
{
    public interface ICallbackService
    {
        Task Send(ShouterEvent message);
    }
}
