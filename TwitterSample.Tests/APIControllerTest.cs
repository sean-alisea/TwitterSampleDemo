using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using TwitterSample.API.Controllers;
using TwitterSample.Models;
using TwitterSample.Services.Cache;

namespace TwitterSample.Tests
{
	[TestClass]
    public class APIControllerTest
	{
        [TestMethod]
        public async Task TestMethod1()
        {
			// Arrange
			Microsoft.Extensions.Logging.Abstractions.NullLogger<TwitterStatisticsController> logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<TwitterStatisticsController>();

			Mock.CacheService cacheService = new Mock.CacheService();

			TwitterStreamStatistics statistics = new TwitterStreamStatistics();

			statistics.TotalTweetsReceived = 100;

			List<KeyValuePair<string, int>> hashTags = new List<KeyValuePair<string, int>>();

			hashTags.Add(new KeyValuePair<string, int>("10", 10));
			hashTags.Add(new KeyValuePair<string, int>("9", 10));
			hashTags.Add(new KeyValuePair<string, int>("8", 10));
			hashTags.Add(new KeyValuePair<string, int>("7", 10));
			hashTags.Add(new KeyValuePair<string, int>("6", 10));
			hashTags.Add(new KeyValuePair<string, int>("5", 10));
			hashTags.Add(new KeyValuePair<string, int>("4", 10));
			hashTags.Add(new KeyValuePair<string, int>("3", 10));
			hashTags.Add(new KeyValuePair<string, int>("2", 10));
			hashTags.Add(new KeyValuePair<string, int>("1", 10));

			statistics.TopTenHashtags = hashTags;

			statistics.LastUpdated = System.DateTime.UtcNow;

			await cacheService.WriteStatisticsAsync(statistics);

			TwitterStatisticsController controller = new TwitterStatisticsController(cacheService, logger);
			
			// Act
			var response = await controller.Get();

			// Assert
			Assert.IsTrue(response.GetType() == typeof(OkObjectResult));

			TwitterStreamStatistics stats = (response as OkObjectResult).Value as TwitterStreamStatistics;

			Assert.IsNotNull(stats);
			Assert.IsTrue(stats.TotalTweetsReceived == 100);
			Assert.IsTrue(stats.TopTenHashtags != null && stats.TopTenHashtags.Count == 10);
		}
	}
}