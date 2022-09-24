using System.Threading.Tasks;
using TwitterSample.Models;

namespace TwitterSample.Services.Cache
{ 
    public interface ICacheService
    {
        public Task<TwitterStreamStatistics> ReadStatisticsAsync();
        public Task WriteStatisticsAsync(TwitterStreamStatistics statistics);
    }
}
