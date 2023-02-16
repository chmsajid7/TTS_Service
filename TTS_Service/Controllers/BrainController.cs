using Microsoft.AspNetCore.Mvc;
using TTS_Service.Services;

namespace TTS_Service.Controllers;

[ApiController]
[Route("api/brain")]
public class BrainController : ControllerBase
{
    private readonly IOpenAIService _openAIService;

    public BrainController(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    [HttpGet("run")]
    public async Task<IActionResult> GenerateResponseAsync(string text)
    {
        var audioData = await _openAIService.GenerateResponseAsync(text).ConfigureAwait(false);
        return File(audioData, "audio/wav");
    }
}
