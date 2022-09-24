namespace TwitterSample.Models
{
    public class TwitterStreamStatistics
    {
        public int TotalTweetsReceived {  get; set; }
        public List<KeyValuePair<string, int>> TopTenHashtags { get; set; } = new List<KeyValuePair<string, int>>();
        public DateTime LastUpdated { get; set; }
    }
}