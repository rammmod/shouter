namespace Rhinero.Shouter.Extensions
{
    internal static class IWebHostEnvironmentExtensions
    {
        public static bool IsTest(this IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(hostEnvironment, "hostEnvironment");
            return hostEnvironment.IsEnvironment("Test");
        }
    }
}
