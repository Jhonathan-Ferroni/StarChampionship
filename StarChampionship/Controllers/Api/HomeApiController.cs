using Microsoft.AspNetCore.Mvc;

namespace StarChampionship.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { app = "StarChampionship API", version = "v1", message = "Welcome to the StarChampionship API" });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
        }
    }
}
