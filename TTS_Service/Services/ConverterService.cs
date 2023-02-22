using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using TTS_Service.Context;
using TTS_Service.Models;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace TTS_Service.Services;

public class ConverterService : IConverterService
{
    private readonly ILogger<ConverterService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly SpeechConfig _speechConfig;
    private readonly ConverterDbContext _dbContext;
    private readonly MemoryCacheEntryOptions _memoryCacheOptions;
    private readonly HttpClient _client;

    private readonly string _language;

    public ConverterService(
        IMemoryCache memoryCache,
        ConverterDbContext dbContext,
        SpeechConfig speechConfig,
        IConfiguration configuration,
        ILogger<ConverterService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri($"https://{configuration.GetValue<string>("CognitiveService:Region")}.stt.speech.microsoft.com/" +
            $"speech/recognition/conversation/cognitiveservices/v1" +
            $"?language={configuration.GetValue<string>("CognitiveService:Language")}");
        _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetValue<string>("CognitiveService:SubscriptionKey"));
        _configuration = configuration;
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _memoryCacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetPriority(CacheItemPriority.Normal);
        _language = configuration.GetValue<string>("CognitiveService:Language");
        _speechConfig = speechConfig;
        _logger = logger;
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
            return "NotValid";
        }

        using var ms = new MemoryStream();
        await audio.CopyToAsync(ms);

        var audioContent = new ByteArrayContent(ms.ToArray());
        audioContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

        var response = await _client.PostAsync(string.Empty, audioContent);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<SttResult>(responseContent);

            return result.DisplayText;
        }

        return "";





        //if (audio is null || !audio.ContentType.Equals("audio/wave"))
        //{
        //    return null;
        //}

        //using var audioStream = new MemoryStream();
        //await audio.CopyToAsync(audioStream);
        //audioStream.Position = 0;

        //var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);

        //var pushStream = AudioInputStream.CreatePushStream(audioFormat);
        //pushStream.Write(audioStream.ToArray());
        //pushStream.Close();

        //var audioConfig = AudioConfig.FromStreamInput(pushStream);

        //using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
        //var result = await recognizer.RecognizeOnceAsync();

        //if (result.Reason == ResultReason.Canceled)
        //{
        //    var cancellation = CancellationDetails.FromResult(result);
        //    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

        //    if (cancellation.Reason == CancellationReason.Error)
        //    {
        //        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
        //        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
        //        Console.WriteLine($"CANCELED: Did you update the subscription info?");
        //    }
        //}

        //string transcribedText = result.Text;

        //return transcribedText;
    }

    // HELPER METHODS

    private async Task<byte[]> GetSpeechAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return default;
        }

        using (var synthesizer = new SpeechSynthesizer(_speechConfig, null as AudioConfig))
        {
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var audioData = result.AudioData;
                    return audioData;
                }
                else
                {
                    return default;
                }
            }
        }
    }
}
