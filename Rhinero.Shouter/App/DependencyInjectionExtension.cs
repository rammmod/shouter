using Microsoft.Extensions.DependencyInjection.Extensions;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Services;
using Rhinero.Shouter.Shared;

namespace Rhinero.Shouter.App
{
    internal static class DependencyInjectionExtension
    {
        internal static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddKeyedScoped<ICallbackService, HttpCallbackService>(ProtocolEnum.Http);
            services.TryAddKeyedScoped<ICallbackService, GrpcCallbackService>(ProtocolEnum.Grpc);

            services.AddHttpClient();
            //TODO: add services

            services.InitializeMassTransit(configuration);
        }
    }
}
