using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

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
            new ChatRequestSystemMessage("Du bist ein freundlicher, fröhlicher chat-assistent."),
            new ChatRequestSystemMessage($"Heute ist {DateTime.Now:D}."),
        });

        // Use manual entry of the tool
        chatCompletionsOptions.Tools.Add(new ChatCompletionsFunctionToolDefinition()
        {
            Name = "GetEmployeeId",
            Description = "Get the employee id of a person",
            Parameters = BinaryData.FromObjectAsJson(new {
                Type = "object",
                Properties = new {
                    FullName = new {
                        Type = "string",
                        Description = "The full name of the person with first and last name",
                    }
                },
                Required = new [] { "FullName", },
            },
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, })
        });

        // use reflection to add another tool
        chatCompletionsOptions.Tools.Add(GetType().GetMethod("BookSlot")!.ToDefinition());

        bool wasToolCall = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!wasToolCall)
            {
                // Get user input
                Console.Write("Your input: ");
                var input = Console.ReadLine();

                // in case it's empty (enter, or ctrl-c), we just ignore it
                if (String.IsNullOrEmpty(input)) continue;

                // add it to chat history
                chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(input));
            }
            wasToolCall = false;

            // get response from LLM
            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, stoppingToken);
            var responseChoice = response.Value.Choices[0];

            if (responseChoice.FinishReason == CompletionsFinishReason.ToolCalls)
            {
                // add assistant message with tool call to chat history
                chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(responseChoice.Message));
                
                // perform the tool call and add response to message
                foreach (var call in responseChoice.Message.ToolCalls)
                {
                    chatCompletionsOptions.Messages.Add(GetToolCallResponseMessage(call));
                }

                wasToolCall = true;
            }
            else
            {
                // Is normal response
                var answer = responseChoice.Message.Content;

                // add response to chat history and write it to the user
                chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(answer));
                Console.WriteLine($"AI output: {answer}");
            }
        }
    }

    private ChatRequestToolMessage GetToolCallResponseMessage(ChatCompletionsToolCall toolCall)
    {
        var functionToolCall = toolCall as ChatCompletionsFunctionToolCall;

        // manual tool calling
        if (functionToolCall?.Name == "GetEmployeeId")
        {
            string unvalidatedArguments = functionToolCall.Arguments;
            Console.WriteLine($"AI called function: {functionToolCall.Name} with arguments: {unvalidatedArguments}");

            return new ChatRequestToolMessage("Employee Id: 42", functionToolCall.Id);
        }
        else if (functionToolCall is not null)
        {
            // try to find a tool
            var toolMethodInfo = this.GetType().GetMethods().FirstOrDefault(m => m.Name == functionToolCall.Name);
            if (toolMethodInfo is not null && functionToolCall is not null)
            {
                string unvalidatedArguments = functionToolCall.Arguments;
                var arguments = JsonSerializer.Deserialize<JsonElement>(unvalidatedArguments);
                
                var parameters = new List<object>();
                foreach (var parameter in toolMethodInfo.GetParameters())
                {
                    if (arguments.TryGetProperty(parameter.Name!, out var argument))
                    {
                        parameters.Add(argument.Deserialize(parameter.ParameterType)!);
                    }
                }

                var result = toolMethodInfo.Invoke(this, parameters.ToArray());

                return new ChatRequestToolMessage(result!.ToString(), functionToolCall.Id);
            }
        }

        throw new NotImplementedException();
    }

    [Description("Books a meeting slot for another employee at a given date for a given length of time.")]
    public string BookSlot(
        [Description("The employee id of the person to book a slot for.")]
        int employeeId,

        [Description("The title of the booking slot, Example: 'Meeting with John', 'Lunch with Customer'")]
        string bookingReason,
        
        [Description("The datetime when the slot should start, in ISO 8601 format. Example: 2021-09-01T10:00:00+02:00")]
        string startDate,
        
        [Description("The length of the slot in minutes.")]
        int slotLength)
    {
        Console.WriteLine($"AI called function: {nameof(BookSlot)} with arguments: {employeeId}, {bookingReason}, {startDate}, {slotLength}");
        return $"Booked '{bookingReason}' slot for employee {employeeId} on {startDate:D} for {slotLength} minutes.";
    }
}

public static class ReflectionExtensions
{
    public static ChatCompletionsFunctionToolDefinition ToDefinition(this MethodInfo methodInfo)
    {
        var toolDefinition = new ChatCompletionsFunctionToolDefinition()
        {
            Name = methodInfo.Name,
            Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? methodInfo.Name,
        };

        var parameters = methodInfo.GetParameters();
        if (parameters.Length > 0)
        {
            toolDefinition.Parameters = BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = parameters.ToDictionary(p => p.Name!, p => new
                {
                    Type = p.ParameterType.ToJsonSchemaType(),
                    Description = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name,
                }),
                Required = parameters.Select(p => p.Name!).ToArray(),
            },
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });
        }

        return toolDefinition;
    }

    public static string ToJsonSchemaType(this Type type)
    {
        // capture most common types
        if (type == typeof(int)) return "integer";
        if (type == typeof(long)) return "integer";
        if (type == typeof(float)) return "number";
        if (type == typeof(double)) return "number";
        if (type == typeof(decimal)) return "number";
        if (type == typeof(bool)) return "boolean";

        return "string";
    }
}
