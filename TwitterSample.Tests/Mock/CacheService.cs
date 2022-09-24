using System.Threading.Tasks;
using TwitterSample.Models;
using TwitterSample.Services.Cache;

namespace TwitterSample.Tests.Mock
{
    public class CacheService : ICacheService
    {
        private TwitterStreamStatistics? _statistics;        

        public async Task<TwitterStreamStatistics> ReadStatisticsAsync()
        {
            return this._statistics;
        }

        public async Task WriteStatisticsAsync(TwitterStreamStatistics statistics)
        {
            this._statistics = statistics;
        }
    }
}
