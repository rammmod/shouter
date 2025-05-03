namespace Rhinero.Shouter.Shared.RetryOptions
{
    internal enum RetryStrategy
    {
        None,
        Immediate,
        Interval,
        Exponential,
        Incremental
    }
}
