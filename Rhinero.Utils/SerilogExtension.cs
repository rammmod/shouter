using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Rhinero.Utils.Logging
{
    public static class SerilogExtension
    {
        public static void AddSerilog(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Host.UseSerilog();

            Log.Logger = new LoggerConfiguration()
                   //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                   //.Enrich.FromLogContext()
                   //.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information) // comment this
                   //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticLogConfig.Uri)
                   //{
                   //    AutoRegisterTemplate = true,
                   //    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                   //    IndexFormat = @$"{elasticLogConfig.Index}-{DateTime.UtcNow:dd-MM-yyyy}",
                   //})
                   .ReadFrom.Configuration(configuration)
                   .CreateLogger();
        }


        //public static void AddElasticLog(this IServiceCollection services, IConfiguration configuration)
        //{
        //    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        //    //var configuration = new ConfigurationBuilder()
        //    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //    //    .AddJsonFile(
        //    //        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        //    //        optional: true)
        //    //    .Build();
        //    var uri = configuration["ElasticConfiguration:Uri"];
        //    Log.Logger = new LoggerConfiguration()
        //        .Enrich.FromLogContext()
        //        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
        //        {
        //            AutoRegisterTemplate = true,
        //            IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower()}-{DateTime.UtcNow:yyyy-MM}"
        //        })
        //        .Enrich.WithProperty("Environment", environment)
        //        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose)
        //        .ReadFrom.Configuration(configuration)
        //        .CreateLogger();
        //}

        //public static void UseElasticLog(this IApplicationBuilder app)
        //{
        //    //this.UseSerilog();
        //}
    }
}
