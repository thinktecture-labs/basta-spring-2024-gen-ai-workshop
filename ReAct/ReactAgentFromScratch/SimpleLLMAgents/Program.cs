using SimpleLLMAgents;
using SimpleLLMAgents.Tools;

var llm = new ChatLlm();
//var response = await llm.Generate("Who was the first president of the USA?");

var agent = new ReActAgent(llm, new List<ITool> { new CalculatorTool() });
var response = await agent.Run("what is 3 + 5 / 7 * 9 ?");

Console.WriteLine("### " + response);