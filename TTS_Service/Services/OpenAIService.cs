using Google.Protobuf.WellKnownTypes;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;

namespace TTS_Service.Services;

public class OpenAIService : IOpenAIService
{
    private const string COMPLETIONS_MODEL = "text-davinci-002";
    private const string OPENAI_API_KEY = "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";

    private readonly IConverterService _converterService;

    public OpenAIService(IConverterService converterService)
    {
        _converterService = converterService;
    }

    public async Task<string> GenerateResponseAsync(string text)
    {
        var openAiApi = new OpenAIAPI(OPENAI_API_KEY);

        string speech = "";

        await foreach (var token in openAiApi.Completions.StreamCompletionEnumerableAsync(new CompletionRequest(
            prompt: text,
            model: Model.DavinciText,
            max_tokens: 300,
            temperature: 0.5,
            presencePenalty: 0.1, 
            frequencyPenalty: 0.1)))
        {
            string word = token.ToString();
            speech = $"{speech} {word}";

            if (word.Contains("/n") || word.Contains(".") || word.Contains(",") || word.Contains(":"))
            {
                await _converterService.ConvertToSpeech(speech);
                speech = "";
            }
        }



        var completionRequest = new CompletionRequest
        {
            Model = "text-davinci-003",
            Prompt = "My name is Roger and I am a principal software engineer at Salesforce.  This is my resume:",
            MaxTokens = 300,
            Temperature = 0.5,
            PresencePenalty= 0.1,
            FrequencyPenalty= 0.1,
        };

        var response = await openAiApi.Completions.CreateCompletionAsync(completionRequest).ConfigureAwait(false);

        var res = response.Completions.FirstOrDefault().Text;

        return res;
    }
}
