using Rhinero.Shouter.App;
using Rhinero.Shouter.ExceptionHandler;
using Rhinero.Shouter.Extensions;
using Rhinero.Shouter.Interfaces;
using Rhinero.Utils.Logging;

namespace Rhinero.Shouter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Test"))
                Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var configuration = builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .Build();

            builder.AddSerilog();

            builder.Services.AddServices(configuration);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.IsTest())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Services.InitializeServices();

            app.Run();
        }
    }
}