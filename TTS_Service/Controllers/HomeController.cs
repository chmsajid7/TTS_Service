using Microsoft.AspNetCore.Mvc;

namespace TTS_Service.Controllers;

[Route("ping")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpPost]
    public IActionResult Ping()
    {
        return Ok("App is up and running!");
    }
}
