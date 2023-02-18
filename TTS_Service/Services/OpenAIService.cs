using Google.Protobuf.WellKnownTypes;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;

namespace TTS_Service.Services;

public class OpenAIService : IOpenAIService
{
    private readonly string _openApiKey;

    public OpenAIService(IConfiguration configuration)
    {
        _openApiKey = configuration.GetValue<string>("OpenAI:Key");
    }

    public async Task<string> GenerateResponseAsync(string text)
    {
        var openAiApi = new OpenAIAPI(_openApiKey);

        var completionRequest = new CompletionRequest
        {
            Model = "text-davinci-003",
            Prompt = text,
            MaxTokens = 300,
            Temperature = 0.5,
            PresencePenalty= 0.1,
            FrequencyPenalty= 0.1,
        };

        string result = "";

        await foreach (var token in openAiApi.Completions.StreamCompletionEnumerableAsync(completionRequest))
        {
            string word = token.ToString();
            result = $"{result} {word}";
        }

        return result;
    }
}
