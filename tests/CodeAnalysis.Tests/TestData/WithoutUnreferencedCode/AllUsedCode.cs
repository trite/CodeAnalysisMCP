namespace WithoutUnreferencedCode;

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
}

public class CalculatorUser
{
    private readonly Calculator _calculator = new();

    public int PerformCalculations()
    {
        var sum = _calculator.Add(1, 2);
        var diff = _calculator.Subtract(5, 3);
        var product = _calculator.Multiply(4, 5);
        return sum + diff + product;
    }
}

public static class Program
{
    public static void Main()
    {
        var user = new CalculatorUser();
        var result = user.PerformCalculations();
    }
}
