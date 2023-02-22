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
        var result = await _openAIService.GenerateResponseAsync(text).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RunProcessAsync(IFormFile speech)
    {
        var text = await _converterService.ConvertToText(speech).ConfigureAwait(false);

        if (text == "NotValid")
        {
            return BadRequest("Speech must be a valid wav format audio");
        }

        if (text is null)
        {
            return Ok("Unable to recogize your speech");
        }

        if (text == "")
        {
            return Ok("Unable to recogize your speech");
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
