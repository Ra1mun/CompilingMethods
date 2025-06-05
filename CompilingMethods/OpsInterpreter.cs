namespace CompilingMethods;

using System;
using System.Collections.Generic;
using System.Linq;

public class OpsInterpreter
{
    private const double Eps = 1e-9;
    private readonly Stack<double> _stack = new();
    private readonly Dictionary<string, double> _variables = new();
    private readonly Dictionary<string, int> _labels = new();
    private readonly List<Operation> _program;
    private int _pc;

    // Память для массивов
    private int _memPointer;
    private const int MemSize = 3000;
    private readonly List<object> _memory = new();

    public OpsInterpreter(List<Operation> program)
    {
        _program = program;
        _pc = 0;
        PreprocessLabels();
    }

    private void PreprocessLabels()
    {
        for (int i = 0; i < _program.Count; i++)
        {
            var op = _program[i];
            if (op.Type == OperationType.Label)
            {
                _labels[op.Value] = i;
            }
        }
    }

    public void Execute()
    {
        while (_pc < _program.Count)
        {
            var op = _program[_pc++];
            ExecuteOperation(op);
        }
    }

    private void ExecuteOperation(Operation op)
    {
        switch (op.Type)
        {
            case OperationType.Number:
                _stack.Push(double.Parse(op.Value));
                break;
            case OperationType.Variable:
                if (!_variables.TryGetValue(op.Value, out double value))
                {
                    throw new Exception($"Переменная не инициализирована: {op.Value}");
                }
                _stack.Push(value);
                break;
            case OperationType.Operator:
                ExecuteOperator(op.Value);
                break;
            case OperationType.Label:
                // Метки обрабатываются в PreprocessLabels
                break;
            case OperationType.Jump:
                Jump(op.Value);
                break;
            case OperationType.JumpIfFalse:
                JumpIfFalse(op.Value);
                break;
            case OperationType.Print:
                Console.WriteLine(_stack.Pop());
                break;
            case OperationType.Read:
                Console.Write("Введите значение: ");
                var input = double.Parse(Console.ReadLine());
                _variables[op.Value] = input;
                break;
            case OperationType.Assign:
                var val = _stack.Pop();
                _variables[op.Value] = val;
                break;
            default:
                throw new Exception($"Неизвестная операция: {op.Type}");
        }
    }

    private void ExecuteOperator(string op)
    {
        switch (op)
        {
            case "+":
                BinaryOp((a, b) => a + b);
                break;
            case "-":
                BinaryOp((a, b) => a - b);
                break;
            case "*":
                BinaryOp((a, b) => a * b);
                break;
            case "/":
                BinaryOp((a, b) => b == 0 ? throw new DivideByZeroException() : a / b);
                break;
            case ">":
                CompareOp((a, b) => a > b);
                break;
            case "<":
                CompareOp((a, b) => a < b);
                break;
            case "#":
                CompareOp((a, b) => Math.Abs(a - b) < 1e-10);
                break;
            case "!":
                CompareOp((a, b) => Math.Abs(a - b) >= 1e-10);
                break;
            case "&":
                BinaryOp((a, b) => (a != 0 && b != 0) ? 1 : 0);
                break;
            case "|":
                BinaryOp((a, b) => (a != 0 || b != 0) ? 1 : 0);
                break;
            case "~":
                UnaryOp(a => -a);
                break;
            case "[]":
                // Обработка индексации массива
                var index = (int)_stack.Pop();
                var array = _stack.Pop();
                // TODO: Реализовать работу с массивами
                break;
            default:
                throw new Exception($"Неизвестный оператор: {op}");
        }
    }

    private void BinaryOp(Func<double, double, double> op)
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        _stack.Push(op(a, b));
    }

    private void CompareOp(Func<double, double, bool> op)
    {
        var b = _stack.Pop();
        var a = _stack.Pop();
        _stack.Push(op(a, b) ? 1 : 0);
    }

    private void UnaryOp(Func<double, double> op)
    {
        var a = _stack.Pop();
        _stack.Push(op(a));
    }

    private void Jump(string label)
    {
        if (!_labels.TryGetValue(label, out int target))
        {
            throw new Exception($"Метка не найдена: {label}");
        }
        _pc = target;
    }

    private void JumpIfFalse(string label)
    {
        var condition = _stack.Pop();
        if (condition == 0)
        {
            if (!_labels.TryGetValue(label, out int target))
            {
                throw new Exception($"Метка не найдена: {label}");
            }
            _pc = target;
        }
    }

    private double GetValue(OpsItem item)
    {
        return item.Type switch
        {
            5 => (double)item.Value,
            6 => Convert.ToDouble((int)item.Value),
            3 when item.Value is string s && _variables.ContainsKey(s) => _variables[s],
            _ => throw new InvalidOperationException("Невозможно получить значение")
        };
    }

    private int GetTokenType(Token token)
    {
        if (token.Type == "id") return 3;
        if (token.Type == "number") return 5;
        if (Definitions.Keywords.Contains(token.Value)) return 3;
        return 0; // операция
    }

    private double TaylorExp(double x)
    {
        double term = 1.0, result = 1.0;
        for (int i = 1; Math.Abs(term) > Eps; i++)
        {
            term *= x / i;
            result += term;
        }
        return result;
    }

    private double TaylorSqrt(double x)
    {
        if (x < 0) throw new ArgumentException("Квадратный корень из отрицательного числа");
        if (x == 0) return 0;

        double guess = x / 2.0;
        double nextGuess;
        do
        {
            nextGuess = 0.5 * (guess + x / guess);
            if (Math.Abs(nextGuess - guess) < Eps) break;
            guess = nextGuess;
        } while (true);

        return guess;
    }
}

public class OpsItem
{
    public object Value { get; set; }
    public int Type { get; set; } // 1 - static mem, 2 - dynamic mem, 3 - variable, 4 - array, 5 - double, 6 - int
    public int LineNo { get; set; }
    public int CharNo { get; set; }
}

internal class ArrayElementReference
{
    private readonly double[] _array;
    private readonly int _index;

    public ArrayElementReference(double[] array, int index)
    {
        _array = array;
        _index = index;
    }

    public override string ToString()
    {
        return _array[_index].ToString();
    }
}