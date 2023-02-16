namespace TTS_Service.Services;

public interface IOpenAIService
{
    Task<string> GenerateResponseAsync(string text);
}
