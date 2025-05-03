using Microsoft.Extensions.Options;

namespace Rhinero.Shouter.Shared.RetryOptions
{
    internal class RetryOptionsValidator : IValidateOptions<RetryOptions>
    {
        public ValidateOptionsResult Validate(string name, RetryOptions options)
        {
            if (options is null)
                return ValidateOptionsResult.Fail("Retry options are required");

            if (options.RetryCount < 0)
                return ValidateOptionsResult.Fail("RetryCount must be >= 0");

            switch (options.Strategy)
            {
                case RetryStrategy.Interval:
                    if (options.IntervalSeconds is null)
                        return ValidateOptionsResult.Fail("IntervalSeconds is required for Interval strategy");
                    break;

                case RetryStrategy.Exponential:
                    var expMissing = new List<string>();
                    if (options.MinIntervalSeconds is null)
                        expMissing.Add(nameof(options.MinIntervalSeconds));

                    if (options.MaxIntervalSeconds is null)
                        expMissing.Add(nameof(options.MaxIntervalSeconds));

                    if (options.IntervalDeltaSeconds is null)
                        expMissing.Add(nameof(options.IntervalDeltaSeconds));

                    if (expMissing.Count != 0)
                        return ValidateOptionsResult.Fail($"{string.Join(", ", expMissing)} are required for Exponential strategy");
                    break;

                case RetryStrategy.Incremental:
                    var incMissing = new List<string>();
                    if (options.InitialIntervalSeconds is null)
                        incMissing.Add(nameof(options.InitialIntervalSeconds));

                    if (options.IncrementSeconds is null)
                        incMissing.Add(nameof(options.IncrementSeconds));

                    if (incMissing.Count != 0)
                        return ValidateOptionsResult.Fail($"{string.Join(", ", incMissing)} are required for Incremental strategy");
                    break;
            }

            return ValidateOptionsResult.Success;
        }
    }
}
