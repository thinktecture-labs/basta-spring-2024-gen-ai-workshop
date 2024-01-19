
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public class ConsoleWorker : BackgroundService
{
    protected readonly OpenAIClient _openAIClient;
    protected readonly string _modelName;

    public ConsoleWorker(OpenAIClient openAIClient, IConfiguration configuration)
    {
        _openAIClient = openAIClient;
        _modelName = configuration["Model"]!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await DoLoopAsync(stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // This is fine.
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An exception occured: {0}", ex);
        }
    }

    private async Task DoLoopAsync(CancellationToken stoppingToken)
    {
        // create chat context and add system prompt
        var chatCompletionsOptions = new ChatCompletionsOptions(_modelName, new [] {
            new ChatRequestSystemMessage("You are a happy, helpful assistant."),
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            // Get user input
            Console.Write("Your input: ");
            var input = Console.ReadLine();

            // in case it's empty (enter, or ctrl-c), we just ignore it
            if (String.IsNullOrEmpty(input)) continue;

            // add it to chat history
            chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(input));
            
            // get response from LLM
            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, stoppingToken);
            var answer = response.Value.Choices[0].Message.Content;

            // add response to chat history and write it to the user
            chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(answer));
            Console.WriteLine($"AI output: {answer}");
        }
    }
}