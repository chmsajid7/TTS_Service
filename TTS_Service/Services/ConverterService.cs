using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NAudio.Wave;
using System.IO;
using TTS_Service.Context;

namespace TTS_Service.Services;

public class ConverterService : IConverterService
{
    private readonly IMemoryCache _memoryCache;
    private readonly SpeechConfig _speechConfig;
    private readonly ConverterDbContext _dbContext;
    private readonly MemoryCacheEntryOptions _memoryCacheOptions;

    public ConverterService(
        IMemoryCache memoryCache,
        ConverterDbContext dbContext,
        SpeechConfig speechConfig)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _memoryCacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetPriority(CacheItemPriority.Normal);
        _speechConfig = speechConfig;
    }

    public async Task<byte[]> ConvertToSpeech(string text)
    {
        text = text.Trim().ToLower();

        if (_memoryCache.TryGetValue(text, out byte[] audioData))
        {
            return audioData;
        }

        var res = await _dbContext.TssModel.FirstOrDefaultAsync(x => x.Text.Equals(text));

        if (res is not null)
        {
            return res.Speech;
        }

        audioData = await GetSpeechAsync(text).ConfigureAwait(false);

        _memoryCache.Set(text, audioData, _memoryCacheOptions);

        return audioData;
    }

    public async Task<byte[]> ConvertToSpeechAndSave(string text)
    {
        text = text.Trim().ToLower();

        var res = await _dbContext.TssModel.FirstOrDefaultAsync(x => x.Text.Equals(text));

        if (res is not null)
        {
            return res.Speech;
        }

        var audioData = await GetSpeechAsync(text).ConfigureAwait(false);

        _dbContext.TssModel.Add(new Models.TssModel
        {
            Text = text,
            Speech = audioData,
        });
        await _dbContext.SaveChangesAsync();

        return audioData;
    }

    public async Task<string> ConvertToText(IFormFile audio)
    {
        if (audio is null || !audio.ContentType.Equals("audio/wave"))
        {
            return null;
        }

        using var audioStream = new MemoryStream();
        await audio.CopyToAsync(audioStream);
        audioStream.Position = 0;
        //


        


        //


        var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1));
        pushStream.Write(audioStream.ToArray());

        var audioConfig = AudioConfig.FromStreamInput(pushStream);

        using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
        var result = await recognizer.RecognizeOnceAsync();

        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine($"Transcription: {result.Text}");
        }
        else if (result.Reason == ResultReason.NoMatch)
        {
            Console.WriteLine($"No speech was recognized.");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            Console.WriteLine($"Recognition was canceled. Reason: {cancellation.Reason}. Error Details: {cancellation.ErrorDetails}");
        }

        return result.Text;
    }

    // HELPER METHODS

    private async Task<byte[]> GetSpeechAsync(string text)
    {
        using (var synthesizer = new SpeechSynthesizer(_speechConfig))
        {
            using (var result = await synthesizer.StartSpeakingTextAsync(text).ConfigureAwait(false))
            {
                return result.AudioData;
            }
        }
    }
}
