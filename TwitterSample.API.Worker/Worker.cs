using TwitterSample.Services.Cache;
using TwitterSample.API.Worker.Services.Twitter;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ITwitterService _twitterService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<Worker> _logger;

        public Worker(ITwitterService twitterService, ICacheService cacheService, ILogger<Worker> logger)
        {
            this._twitterService = twitterService;
            this._cacheService = cacheService;
            this._logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await this._twitterService.ProcessTweetsAsync(this._cacheService, this._logger);
                }
                catch (HttpRequestException e)
                {
                    this._logger.LogError(e.ToString());

                    // Handle throttling at this level
                    if (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // Pause for 5 minutes then start over again (not ideal...need a circuit-breaker
                        // at the point of stream read but wasn't sure how to implement that within
                        // the time I allotted for this).
                        Thread.Sleep(TimeSpan.FromMinutes(5));

                        await this._twitterService.ProcessTweetsAsync(this._cacheService, this._logger);
                    }
                }
                catch (Exception e)
                {
                    this._logger.LogError(e.ToString());

                    throw;
                }
            }
        }
    }
}