using Microsoft.AspNetCore.Mvc;

namespace UnityApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScoreController : ControllerBase
    {
        [HttpPost]
        public IActionResult PostScore([FromBody] ScoreData data)
        {
            Console.WriteLine($"Score ontvangen: {data.Score}");
            return Ok(new { status = "ok", received = data });
        }
    }

    public class ScoreData
    {
        public int Score { get; set; }
    }
}
