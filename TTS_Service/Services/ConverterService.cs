using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TTS_Service.Context;

namespace TTS_Service.Services;

public class ConverterService : IConverterService
{
    private readonly string _subscriptionKey;
    private readonly string _region;

    private readonly IMemoryCache _memoryCache;
    private readonly ConverterDbContext _dbContext;
    private readonly MemoryCacheEntryOptions _memoryCacheOptions;

    public ConverterService(
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ConverterDbContext dbContext)
    {
        _dbContext = dbContext;
        _region = configuration["CognitiveService:Region"]
            ?? throw new ArgumentNullException(nameof(_region));
        _subscriptionKey = configuration["CognitiveService:SubscriptionKey"]
            ?? throw new ArgumentNullException(nameof(_subscriptionKey));
        _memoryCache = memoryCache
            ?? throw new ArgumentNullException(nameof(_memoryCache));
        _memoryCacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetPriority(CacheItemPriority.Normal);
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

    private async Task<byte[]> GetSpeechAsync(string text)
    {
        var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);

        using var synthesizer = new SpeechSynthesizer(config);
        using var result = await synthesizer.SpeakTextAsync(text).ConfigureAwait(false);

        return result.AudioData;
    }
}
