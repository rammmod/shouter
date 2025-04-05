using Rhinero.Shouter.App;
using Rhinero.Utils.Logging;

namespace Rhinero.Shouter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
            {
                Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
            }

            var configuration = builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .Build();

            builder.AddSerilog(configuration);

            builder.Services.AddServices(configuration);

            var app = builder.Build();

            app.Run();
        }
    }
}