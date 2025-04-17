using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Rhinero.Shouter.Interfaces;
using Rhinero.Shouter.Services.Callbacks;
using Rhinero.Shouter.Services.Protos;
using Rhinero.Shouter.Shared;

namespace Rhinero.Shouter.App
{
    internal static class DependencyInjectionExtension
    {
        internal static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; });

            services.Configure<Protoc>(configuration.GetSection(nameof(Protoc)));
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<Protoc>>().Value);

            services.TryAddKeyedScoped<ICallbackService, HttpCallbackService>(ProtocolEnum.Http);
            services.TryAddKeyedScoped<ICallbackService, GrpcCallbackService>(ProtocolEnum.Grpc);

            services.TryAddScoped<IProtoRegistration, ProtoRegistration>();

            services.TryAddSingleton<IProtoCache, ProtoCache>();

            services.AddHttpClient();
            //TODO: add services

            services.AddMassTransit(configuration);

            return services;
        }

        internal static IServiceProvider InitializeServices(this IServiceProvider provider)
        {
            provider.GetRequiredService<IProtoCache>();

            return provider;
        }
    }
}
