using TwitterSample.Services.Cache;

namespace TwitterSample.API.Worker.Services.Twitter
{
    public interface ITwitterService
    {
        public Task ProcessTweetsAsync(ICacheService cacheService, ILogger logger);
    }
}
