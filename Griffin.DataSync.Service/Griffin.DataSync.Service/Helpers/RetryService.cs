using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.DataSync.Service.Helpers
{
    public class RetryService
    {
        private readonly AsyncRetryPolicy _policy;

        public RetryService(ILogger<RetryService> logger)
        {
            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, delay, retryCount, _) =>
                    {
                        logger.LogWarning(
                            exception,
                            "Retry {Retry} after {Delay} seconds.",
                            retryCount,
                            delay.TotalSeconds);
                    });
        }

        public Task ExecuteAsync(Func<Task> action)
        {
            return _policy.ExecuteAsync(action);
        }

        public Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            return _policy.ExecuteAsync(action);
        }
    }
}
