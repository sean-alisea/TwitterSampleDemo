using System.Net;
using System.Text;
using System.Text.Json;
using System.Timers;
using Polly;
using Polly.Retry;
using TwitterSample.Models;
using TwitterSample.Services.Cache;
using TwitterSample.API.Worker.Models;

namespace TwitterSample.API.Worker.Services.Twitter
{
    public class TwitterService : ITwitterService
    {
        private readonly string? _twitterAuthUrl;
        private readonly string? _twitterApiUrl;
        private readonly string? _twitterKey;
        private readonly string? _twitterSecret;
        private ICacheService? _cacheService;
        private ILogger? _logger;
        private Dictionary<string, int> _hashtags = new Dictionary<string, int>();
        private TwitterStreamStatistics _statistics = new TwitterStreamStatistics();

        public TwitterService(string twitterAuthUrl, string twitterApiUrl, string twitterKey, string twitterSecret)
        {
            this._twitterAuthUrl = twitterAuthUrl;
            this._twitterApiUrl = twitterApiUrl;
            this._twitterKey = twitterKey;
            this._twitterSecret = twitterSecret;
        }

        public async Task ProcessTweetsAsync(ICacheService cacheService, ILogger logger)
        {
            this._cacheService = cacheService;
            this._logger = logger;

            using (var client = new HttpClient())
            {
                AccessToken token = await GetAccessTokenAsync();

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.access_token}");

                try
                {
                    // Back-off re-try policy for transient errors
                    var retryPolicy = new TransientFailureRetryPolicy<Stream>();

                    using (Stream stream = await retryPolicy.ExecuteAsync(() => client.GetStreamAsync($"{this._twitterApiUrl}?tweet.fields=entities")))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            await this._cacheService.WriteStatisticsAsync(this._statistics);

                            // Set a timer for a 10-second interval to update the cache
                            System.Timers.Timer timer = new System.Timers.Timer(10000);
                            timer.Elapsed += OnTimedEvent;
                            timer.Start();

                            while (!reader.EndOfStream)
                            {
                                string? currentLine = reader.ReadLine();

                                if (!string.IsNullOrEmpty(currentLine))
                                {
                                    TweetInfo? tweet = JsonSerializer.Deserialize<TweetInfo>(currentLine);

                                    if (tweet != null)
                                    {
                                        this._statistics.TotalTweetsReceived++;                                        

                                        if (tweet.Data != null && tweet.Data.Entities != null && tweet.Data.Entities.HashTags != null && tweet.Data.Entities.HashTags.Length > 0)
                                        {
                                            foreach (Hashtag h in tweet.Data.Entities.HashTags)
                                            {
                                                if (!string.IsNullOrEmpty(h.Text) && this._hashtags.Keys.Contains(h.Text))
                                                    this._hashtags[h.Text]++;
                                                else
                                                    this._hashtags.Add(h.Text, 1);
                                            }                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this._logger.LogError(e.ToString());
                    throw;
                }                
            }
        }

        #region Private

        private async Task<AccessToken> GetAccessTokenAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string body = "grant_type=client_credentials&scope=public";

                    StringContent content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {GetBasicAuthCredentialString()}");

                    // Back-off re-try policy for transient errors
                    var retryPolicy = new TransientFailureRetryPolicy<HttpResponseMessage>();

                    using (HttpResponseMessage response = await retryPolicy.ExecuteAsync(() => client.PostAsync(this._twitterAuthUrl, content)))
                    {
                        response.EnsureSuccessStatusCode();

                        string? jsonString = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(jsonString))
                            return JsonSerializer.Deserialize<AccessToken>(jsonString);
                        else
                            return null;
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError($"Unable to fetch Twitter API access token. Error: {e.Message}");
                throw;
            }
        }

        private string GetBasicAuthCredentialString()
        {
            var plainTextBytes = Encoding.UTF8.GetBytes($"{this._twitterKey}:{this._twitterSecret}");
            return Convert.ToBase64String(plainTextBytes);
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
            try
            {
                this._statistics.TopTenHashtags.Clear();

                var orderedDictionary = this._hashtags.OrderByDescending(u => u.Value).Take(10);                

                foreach (KeyValuePair<string, int> kv in orderedDictionary)
                {
                    this._statistics.TopTenHashtags.Add(kv);
                }

                this._statistics.LastUpdated = DateTime.UtcNow;

                this._cacheService.WriteStatisticsAsync(this._statistics);                

                Console.WriteLine(JsonSerializer.Serialize(this._statistics));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
            }
        }

        #endregion
    }

    #region Classes

    class AccessToken
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public long expires_in { get; set; }
    }

    class TransientFailureRetryPolicy<T>
    {
        private const int MaxRetryCount = 5;

        private const double DurationBetweenRetries = 250;

        private readonly AsyncRetryPolicy<T> policy;

        public TransientFailureRetryPolicy()
        {
            this.policy = Policy
                .Handle<HttpRequestException>(e =>
                    e.StatusCode == HttpStatusCode.RequestTimeout ||
                    e.StatusCode == HttpStatusCode.BadGateway ||
                    e.StatusCode == HttpStatusCode.GatewayTimeout ||
                    e.StatusCode == HttpStatusCode.ServiceUnavailable
                 )
                .Or<TaskCanceledException>()
                .OrResult<T>(s => s == null)
                .WaitAndRetryAsync(
                    MaxRetryCount,
                    retryCount => TimeSpan.FromMilliseconds(DurationBetweenRetries * Math.Pow(2, retryCount - 1))
                );
        }

        public async Task<T> ExecuteAsync(Func<Task<T>> action)
        {
            return await policy.ExecuteAsync(action);
        }
    }

    #endregion
}
