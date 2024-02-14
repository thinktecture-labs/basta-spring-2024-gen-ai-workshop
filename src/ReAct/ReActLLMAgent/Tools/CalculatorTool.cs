using System.Globalization;
using AngouriMath;

namespace SimpleLLMAgents.Tools;

public class CalculatorTool : ITool
{
    public string Name => "CalculatorTool";

    public string Description => "Very simple calculator to do basic math operations";

    public string Execute(string inputText)
    {
        Entity expr = inputText;

        return ((decimal)expr.EvalNumerical()).ToString(CultureInfo.InvariantCulture);
    }
}