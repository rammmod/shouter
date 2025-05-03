namespace Rhinero.Shouter.Shared.RetryOptions
{
    internal class RetryOptions
    {
        public RetryStrategy Strategy { get; set; } = RetryStrategy.None;

        public int RetryCount { get; set; } = 5;

        // Interval
        public int? IntervalSeconds { get; set; }

        // Exponential
        public int? MinIntervalSeconds { get; set; }
        public int? MaxIntervalSeconds { get; set; }
        public int? IntervalDeltaSeconds { get; set; }

        // Incremental
        public int? InitialIntervalSeconds { get; set; }
        public int? IncrementSeconds { get; set; }
    }
}
