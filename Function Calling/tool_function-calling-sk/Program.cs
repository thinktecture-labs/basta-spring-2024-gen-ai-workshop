using System.Reflection;
using Azure.AI.OpenAI;
using FunctionCallingWithSemanticKernel.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

var kernel = GetKernel();

Console.WriteLine("Hello, I am an AI assistant that can answer simple math questions.");
Console.WriteLine("Please ask me questions like \"What is 2 x 2\" or \"What is square root of 3\" etc.");
Console.WriteLine("To quit, simply type quit.");
Console.WriteLine("");
Console.WriteLine("Now ask me a math question...");

do
{
    var prompt = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(prompt))
    {
        if (prompt.ToLowerInvariant() == "quit")
        {
            Console.WriteLine("Bye.");
            break;
        }
        else
        {
            var function = await SelectTool(prompt);

            if (function != null)
            {
                kernel.Plugins.TryGetFunctionAndArguments(function, out KernelFunction? pluginFunction,
                    out KernelArguments? arguments);

                var result = await kernel.InvokeAsync(pluginFunction!, arguments);

                Console.WriteLine($"RESULT: {result.ToString()}");
            }
            else
            {
                Console.WriteLine("I'm sorry but I am not able to answer your question. I can only answer simple math questions.");
            }
        }
    }
} while (true);


async Task<OpenAIFunctionToolCall?> SelectTool(string prompt)
{
    try
    {
        /*
        var openAiClient =  new OpenAIClient(
                    new Uri("http://localhost:8083/v1"),
                    new Azure.AzureKeyCredential("empty")
                );
        var field = typeof(OpenAIClient).GetField("_isConfiguredForAzureOpenAI", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(openAiClient, false);
        }
        */
        
        var chatCompletionService = new OpenAIChatCompletionService(
            modelId: "gpt-4-turbo-preview",
            //openAIClient: openAiClient);
            apiKey: openAiApiKey ?? string.Empty);
        
        var result = await chatCompletionService.GetChatMessageContentAsync(new ChatHistory(prompt),
            new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
                Temperature = 0
            }, kernel);
        
        var functionCall = ((OpenAIChatMessageContent)result).GetOpenAIFunctionToolCalls().FirstOrDefault();

        return functionCall;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);

        return null;
    }
}

Kernel GetKernel()
{
    var kernelBuilder = Kernel.CreateBuilder();
	
    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromType<CalculatorPlugin>();

    return kernel;
}