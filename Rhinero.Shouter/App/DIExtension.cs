using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Services;

namespace Rhinero.Shouter.App
{
    internal static class DIExtension
    {
        internal static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICallbackService, CallbackService>();

            services.InitializeMassTransit(configuration);
        }
    }
}
