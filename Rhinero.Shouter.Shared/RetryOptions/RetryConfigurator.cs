using MassTransit;

namespace Rhinero.Shouter.Shared.RetryOptions
{
    internal static class RetryConfigurator
    {
        public static void ConfigureRetry(this IReceiveEndpointConfigurator endpoint, RetryOptions options)
        {
            switch (options.Strategy)
            {
                case RetryStrategy.Immediate:
                    endpoint.UseMessageRetry(r => r.Immediate(options.RetryCount));
                    break;

                case RetryStrategy.Interval:
                    endpoint.UseMessageRetry(r =>
                        r.Interval(options.RetryCount, TimeSpan.FromSeconds(options.IntervalSeconds!.Value)));
                    break;

                case RetryStrategy.Exponential:
                    endpoint.UseMessageRetry(r =>
                        r.Exponential(
                            options.RetryCount,
                            TimeSpan.FromSeconds(options.MinIntervalSeconds!.Value),
                            TimeSpan.FromSeconds(options.MaxIntervalSeconds!.Value),
                            TimeSpan.FromSeconds(options.IntervalDeltaSeconds!.Value)));
                    break;

                case RetryStrategy.Incremental:
                    endpoint.UseMessageRetry(r =>
                        r.Incremental(
                            options.RetryCount,
                            TimeSpan.FromSeconds(options.InitialIntervalSeconds!.Value),
                            TimeSpan.FromSeconds(options.IncrementSeconds!.Value)));
                    break;

                case RetryStrategy.None:
                default:
                    break;
            }
        }
    }
}
