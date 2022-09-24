using System.Text.Json.Serialization;

namespace TwitterSample.API.Worker.Models
{
    public class TweetInfo
    {
        [JsonPropertyName("data")]
        public TweetHeader? Data { get; set; }        
    }    

    public class TweetHeader
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        [JsonPropertyName("entities")]
        public Entities? Entities { get; set; }
    }

    public class Entities
    {
        [JsonPropertyName("hashtags")]
        public Hashtag[]? HashTags { get; set; }
    }

    public class Hashtag
    {
        [JsonPropertyName("tag")]
        public string? Text { get; set; }
    }
}
