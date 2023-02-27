using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using TTS_Service.Services;

namespace TTS_Service.Controllers;

[ApiController]
[Route("api/HeyAlli")]
public class BrainController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly IConverterService _converterService;
    private readonly ILogger<BrainController> _logger;

    public BrainController(IOpenAIService openAIService,
        IConverterService converterService,
        ILogger<BrainController> logger)
    {
        _openAIService = openAIService;
        _converterService = converterService;
        _logger = logger;
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
        _logger.LogInformation($"triggered brain, speech.Length = {speech.Length}");

        var text = await _converterService.ConvertToText(speech).ConfigureAwait(false);

        _logger.LogInformation($"triggered brain, text = {text}");

        if (text == "NotValid")
        {
            return BadRequest("Speech must be a valid wav format audio");
        }

        string result = "Sorry but I was not able to recognize your speech. Please try again";

        if (!string.IsNullOrEmpty(text))
        {
            result = await _openAIService.GenerateResponseAsync(text).ConfigureAwait(false);
        }

        _logger.LogInformation($"triggered brain, result = {result}");

        if (string.IsNullOrEmpty(result))
        {
            return NoContent();
        }

        var audioData = await _converterService.ConvertToSpeech(result).ConfigureAwait(false);

        _logger.LogInformation($"triggered brain, audioData.Length = {audioData.Length}");
        return Ok(audioData);
    }
}
