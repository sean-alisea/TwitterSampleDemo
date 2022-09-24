using Microsoft.AspNetCore.Mvc;
using TwitterSample.Models;
using TwitterSample.Services.Cache;

namespace TwitterSample.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwitterStatisticsController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<TwitterStatisticsController> _logger;

        public TwitterStatisticsController(ICacheService cacheService, ILogger<TwitterStatisticsController> logger)
        {
            this._cacheService = cacheService;
            this._logger = logger;
        }

        [HttpGet(Name = "TwitterStatistics")]
        public async Task<IActionResult> Get()
        {
            try
            {
                TwitterStreamStatistics stats = await this._cacheService.ReadStatisticsAsync();

                if (stats == null)
                    return NotFound();

                return new OkObjectResult(stats);
            }
            catch (Exception e)
            {
                this._logger.LogError(e.ToString());
                return Problem(e.ToString(), null, (int)System.Net.HttpStatusCode.InternalServerError, "Internal Server Error", "Error");
            }            
        }
    }
}