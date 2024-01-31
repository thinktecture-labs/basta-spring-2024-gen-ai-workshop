using System.Globalization;
using System.Text.RegularExpressions;
using SimpleLLMAgents.Tools;

namespace SimpleLLMAgents;

public class ReActAgent
{
    private const string FinalAnswerToken = "Final Answer:";
    private const string ObservationToken = "Observation:";
    private const string ThoughtToken = "Thought:";

    private const int MaxLoops = 15;

    private readonly ChatLlm _llm;

    private readonly List<string> _stopPattern = new() { $"\n{ObservationToken}", $"\n\t{ObservationToken}" };
    private readonly List<ITool> _tools;

    private readonly string PromptTemplate =
        @"Today is {today} and you can use tools to get new information. Answer the question as best as you can using the following tools: 

        {tool_description}

        Use the following format:

        Question: the input question you must answer
        Thought: comment on what you want to do next
        Action: the action to take, exactly one element of [{tool_names}]
        Action Input: the input to the action
        Observation: the result of the action
        ... (this Thought/Action/Action Input/Observation repeats N times, use it until you are sure of the answer)
        Thought: I now know the final answer
        Final Answer: your final answer to the original input question

        Begin!

        Question: {question}
        Thought: {previous_responses}";

    public ReActAgent(ChatLlm llm, List<ITool> tools)
    {
        _llm = llm;
        _tools = tools;
    }

    private string ToolDescription
    {
        get
        {
            var descriptions = _tools.Select(tool => $"{tool.Name}: {tool.Description}").ToList();
            return string.Join("\n", descriptions);
        }
    }

    private string ToolNames
    {
        get
        {
            var names = _tools.Select(tool => tool.Name).ToList();
            return string.Join(",", names);
        }
    }

    private Dictionary<string, ITool> ToolByNames
    {
        get { return _tools.ToDictionary(tool => tool.Name); }
    }

    public async Task<string> Run(string question)
    {
        var previousResponses = new List<string>();
        var numLoops = 0;

        var prompt = PromptTemplate.Replace("{today}", DateTime.Today.ToString(CultureInfo.InvariantCulture))
            .Replace("{tool_description}", ToolDescription)
            .Replace("{tool_names}", ToolNames)
            .Replace("{question}", question)
            .Replace("{previous_responses}", "{previous_responses}");
        Console.WriteLine(prompt.Replace("{previous_responses}", ""));

        while (numLoops < MaxLoops)
        {
            numLoops++;
            var currentPrompt = prompt.Replace("{previous_responses}", string.Join("\n", previousResponses));

            var nextAction = await DecideNextAction(currentPrompt);
            var generated = nextAction.Item1;
            var tool = nextAction.Item2;
            var toolInput = nextAction.Item3;

            if (tool == "Final Answer") return toolInput;

            if (!ToolByNames.ContainsKey(tool)) throw new ArgumentException($"Unknown tool: {tool}");

            var toolResult = ToolByNames[tool].Execute(toolInput);
            generated += $"\n{ObservationToken} {toolResult}\n{ThoughtToken}";

            Console.WriteLine(generated);
            previousResponses.Add(generated);
        }

        return string.Empty;
    }

    private async Task<Tuple<string, string, string>> DecideNextAction(string prompt)
    {
        var generated = await _llm.Generate(prompt, _stopPattern);
        var parsed = ParseAnswer(generated);
        var tool = parsed.Item1;
        var toolInput = parsed.Item2;

        return new Tuple<string, string, string>(generated, tool, toolInput);
    }

    private static Tuple<string, string> ParseAnswer(string generated)
    {
        string toolInput;

        if (generated.Contains(FinalAnswerToken))
        {
            var split = generated.Split(new[] { FinalAnswerToken }, StringSplitOptions.None);
            toolInput = split[split.Length - 1].Trim();

            return new Tuple<string, string>("Final Answer", toolInput);
        }

        var regex = @"Action: [\[]?(.*?)[\]]?[\n]*Action Input:[\s]*(.*)";
        var match = Regex.Match(generated, regex, RegexOptions.Singleline);

        if (!match.Success)
            throw new ArgumentException($"Output of LLM is not parsable for next tool use: `{generated}`");

        var tool = match.Groups[1].Value.Trim();
        toolInput = match.Groups[2].Value.Trim();

        return new Tuple<string, string>(tool, toolInput);
    }
}