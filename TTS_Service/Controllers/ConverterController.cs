using Microsoft.AspNetCore.Mvc;
using TTS_Service.Services;

namespace TTS_Service.Controllers;

[ApiController]
[Route("api/convert")]
public class ConverterController : ControllerBase
{
    private readonly IConverterService _converterService;

    public ConverterController(IConverterService converterService)
    {
        _converterService = converterService;
    }

    [HttpGet("tts")]
    public async Task<IActionResult> ConvertToSpeech(string text)
    {
        var audioData = await _converterService.ConvertToSpeech(text).ConfigureAwait(false);

        return File(audioData, "audio/wav");
    }

    [HttpGet("tts/save")]
    public async Task<IActionResult> ConvertToSpeechAndSave(string text)
    {
        var audioData = await _converterService.ConvertToSpeechAndSave(text).ConfigureAwait(false);

        return File(audioData, "audio/wav");
    }
}
