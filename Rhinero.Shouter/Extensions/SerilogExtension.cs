using Serilog;

namespace Rhinero.Utils.Logging
{
    public static class SerilogExtension
    {
        public static void AddSerilog(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
