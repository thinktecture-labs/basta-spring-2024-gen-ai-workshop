using Azure.AI.OpenAI;


var apiKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (String.IsNullOrEmpty(apiKey))
{
	Console.Error.WriteLine("OPENAI_API_KEY environment variable is not set");
	return;
}

var client = new OpenAIClient(apiKey);
var model = "gpt-4";


var chatCompletionsOptions = new ChatCompletionsOptions(model, new [] {
	new ChatRequestSystemMessage("Dui bist ein freundlicher, fröhlicher chat-assistent."),
});

var completed = false;

Console.CancelKeyPress += (sender, eventArgs) =>
{
	eventArgs.Cancel = true;
	completed = true;
};

while (!completed)
{
	// Get user input
	Console.Write("Your input: ");
	var input = Console.ReadLine();

	// in case it's empty (enter, or ctrl-c), we just ignore it
	if (String.IsNullOrEmpty(input)) continue;

	// add it to chat history
	chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(input));

	// get response from LLM
	var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
	var answer = response.Value.Choices[0].Message.Content;

	// add response to chat history and write it to the user
	chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(answer));
	Console.WriteLine($"AI output: {answer}");
}
