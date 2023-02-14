namespace TTS_Service.Services;

public interface IConverterService
{
    Task<byte[]> ConvertToSpeech(string text);
    Task<byte[]> ConvertToSpeechAndSave(string text);
}
