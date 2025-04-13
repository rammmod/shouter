using Serilog;

namespace Rhinero.Utils.Logging
{
    public static class SerilogExtension
    {
        public static void AddSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}
