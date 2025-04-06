using Rhinero.Shouter.Contracts;
using Rhinero.Shouter.Interfaces;

namespace Rhinero.Shouter.Services
{
    public class GrpcCallbackService : ICallbackService
    {
        public async Task SendAsync(ShouterMessage message)
        {
            await Task.CompletedTask;
        }
    }
}
