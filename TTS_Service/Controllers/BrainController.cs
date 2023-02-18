using Microsoft.AspNetCore.Mvc;
using TTS_Service.Services;

namespace TTS_Service.Controllers;

[ApiController]
[Route("api/HeyAlli")]
public class BrainController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly IConverterService _converterService;

    public BrainController(IOpenAIService openAIService,
        IConverterService converterService)
    {
        _openAIService = openAIService;
        _converterService = converterService;
    }

    [HttpGet("brain")]
    public async Task<IActionResult> GenerateResponseAsync(string text)
    {
        var audioData = await _openAIService.GenerateResponseAsync(text).ConfigureAwait(false);
        return File(audioData, "audio/wav");
    }

    [HttpGet]
    public async Task<IActionResult> RunProcessAsync(IFormFile speech)
    {
        var text = await _converterService.ConvertToText(speech).ConfigureAwait(false);

        if (text is null)
        {
            return BadRequest("Speech must be a valid wav format audio");
        }

        if (text == "")
        {
            return NoContent();
        }

        var result = await _openAIService.GenerateResponseAsync(text).ConfigureAwait(false);

        if (string.IsNullOrEmpty(result))
        {
            return NoContent();
        }

        var audioData = await _converterService.ConvertToSpeech(result).ConfigureAwait(false);
        return File(audioData, "audio/wav");
    }
}
