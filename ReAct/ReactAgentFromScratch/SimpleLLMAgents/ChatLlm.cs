using Azure.AI.OpenAI;

namespace SimpleLLMAgents;

public class ChatLlm
{
    private readonly string _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
    private readonly string _model;
    private readonly float _temperature;

    public ChatLlm(string model = "gpt-3.5-turbo", float temperature = 0.0f)
    {
        _model = model;
        _temperature = temperature;
    }

    public async Task<string> Generate(string prompt, List<string>? stop = null)
    {
        var client = new OpenAIClient(_apiKey);

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            DeploymentName = _model, // Use DeploymentName for "model" with non-Azure clients
            Temperature = _temperature,
            Messages =
            {
                new ChatRequestUserMessage(prompt),
            },
        };

        chatCompletionsOptions.StopSequences.Clear();
        stop?.ForEach(s => chatCompletionsOptions.StopSequences.Add(s));

        var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;

        return responseMessage.Content;
    }
}