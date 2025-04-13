using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Services;
using Rhinero.Shouter.Shared;

namespace Rhinero.Shouter.App
{
    internal static class DependencyInjectionExtension
    {
        internal static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {


            services.Configure<Protos>(configuration.GetSection(nameof(Protos)));
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<Protos>>().Value);

            services.TryAddKeyedScoped<ICallbackService, HttpCallbackService>(ProtocolEnum.Http);
            services.TryAddKeyedScoped<ICallbackService, GrpcCallbackService>(ProtocolEnum.Grpc);

            services.AddHttpClient();
            //TODO: add services

            services.AddMassTransit(configuration);
        }
    }
}
