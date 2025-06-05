namespace CompilingMethods;

using System;
using System.Collections.Generic;
using System.Linq;

public class OpsInterpreter
{
    private const double Eps = 1e-9;
    private const int MaxArrayLength = 1000;
    private readonly Stack<double> _stack = new();
    private readonly Dictionary<string, double> _variables = new();
    private readonly Dictionary<string, int> _labels = new();
    private readonly List<Operation> _program;
    private int _pc;

    // Память для массивов
    private readonly Dictionary<string, double[,]> _arrays = new();
    private int _currentArrayRows;
    private int _currentArrayCols;

    public OpsInterpreter(List<Operation> program)
    {
        _program = program;
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
            case OperationType.String:
                Console.Write(op.Value);
                break;
            case OperationType.Variable:
                if (!_variables.TryGetValue(op.Value, out double value))
                {
                    throw new Exception($"Переменная не инициализирована: {op.Value}");
                }
                _stack.Push(value);
                break;
            case OperationType.ArrayVariable:
                var arrayIndex = (int)_stack.Pop();
                if (!_arrays.TryGetValue(op.Value, out var array))
                {
                    throw new Exception($"Переменная не инициализирована: {op.Value}");
                }
                _stack.Push(array[arrayIndex, 0]);
                break;
            case OperationType.Array2DVariable:
                var indexRow = (int)_stack.Pop();
                var indexCols = (int)_stack.Pop();
                if (!_arrays.TryGetValue(op.Value, out array))
                {
                    throw new Exception($"Переменная не инициализирована: {op.Value}");
                }
                _stack.Push(array[indexRow, indexCols]);
                break;
            case OperationType.Operator:
                ExecuteOperator(op.Value);
                break;
            case OperationType.Function:
                ExecuteFunction(op.Value);
                break;
            case OperationType.ArraySize:
                if (_stack.Count == 0)
                {
                    throw new Exception("Стек пуст при вычислении размера массива");
                }
                var size = (int)Math.Round(_stack.Pop());
                if (size <= 0)
                {
                    throw new Exception("Размер массива должен быть положительным числом");
                }
                if (op.Value == "rows")
                {
                    _currentArrayRows = size;
                    Console.WriteLine($"Установлен размер строк: {size}"); // Отладочный вывод
                }
                else if (op.Value == "cols")
                {
                    _currentArrayCols = size;
                    Console.WriteLine($"Установлен размер столбцов: {size}"); // Отладочный вывод
                }
                else
                {
                    throw new Exception($"Неизвестное измерение массива: {op.Value}");
                }
                break;
            case OperationType.Array1DDecl:
                if (_currentArrayRows <= 0 || _currentArrayCols >= MaxArrayLength)
                {
                    throw new Exception($"Некорректный размер одномерного массива: {_currentArrayRows}");
                }
                var array1DName = op.Value;
                _arrays[array1DName] = new double[_currentArrayRows, 1]; // Одномерный массив как двумерный с одним столбцом
                Console.WriteLine($"Создан одномерный массив {array1DName} размером {_currentArrayRows}"); // Отладочный вывод
                break;
            case OperationType.ArrayDecl:
                if (_currentArrayRows <= 0 || _currentArrayCols <= 0)
                {
                    throw new Exception($"Некорректные размеры двумерного массива: rows={_currentArrayRows}, cols={_currentArrayCols}");
                }
                var array2DName = op.Value;
                _arrays[array2DName] = new double[_currentArrayRows, _currentArrayCols];
                Console.WriteLine($"Создан двумерный массив {array2DName} размером {_currentArrayRows}x{_currentArrayCols}"); // Отладочный вывод
                break;
            case OperationType.ArrayAssign:
                var index = _pc - 1;
                var assignArrayName = _program[index].Value;
                var assignValue = _stack.Pop();
                var assignIndex = (int)_stack.Pop();
                
                if (!_arrays.TryGetValue(assignArrayName, out var assignArray))
                {
                    throw new Exception($"Массив не найден: {assignArrayName}");
                }
                if (assignIndex < 0 || assignIndex >= assignArray.GetLength(0))
                {
                    throw new Exception($"Индекс вне границ массива: {assignIndex}");
                }
                assignArray[assignIndex, 0] = assignValue;
                break;
            case OperationType.Array2DAssign:
                // Присваивание значения элементу двумерного массива
                var assignValue2D = _stack.Pop();
                var assignColIndex = (int)_stack.Pop();
                var assignRowIndex = (int)_stack.Pop();
                var assignArrayName2D = _program[--_pc].Value;
                if (!_arrays.TryGetValue(assignArrayName2D, out var assignArray2D))
                {
                    throw new Exception($"Массив не найден: {assignArrayName2D}");
                }
                if (assignRowIndex < 0 || assignRowIndex >= assignArray2D.GetLength(0) ||
                    assignColIndex < 0 || assignColIndex >= assignArray2D.GetLength(1))
                {
                    throw new Exception($"Индекс вне границ массива: [{assignRowIndex}, {assignColIndex}]");
                }
                assignArray2D[assignRowIndex, assignColIndex] = assignValue2D;
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
                if (_stack.Count > 0)
                {
                    Console.WriteLine(_stack.Pop());
                }
                break;
            case OperationType.Read:
                Console.Write("Введите значение: ");
                var input = Console.ReadLine();
                if (!double.TryParse(input, out var inputValue))
                {
                    throw new Exception($"Некорректное числовое значение: {input}");
                }
                _variables[op.Value] = inputValue;
                Console.WriteLine($"Установлено значение {op.Value} = {inputValue}"); // Отладочный вывод
                break;
            case OperationType.Assign:
                var val = _stack.Pop();
                _variables[op.Value] = val;
                break;
            default:
                throw new Exception($"Неизвестная операция: {op.Type}");
        }
    }

    private void ExecuteFunction(string func)
    {
        var arg = _stack.Pop();
        switch (func)
        {
            case "КОР":
                if (arg < 0) throw new Exception("Отрицательное число под корнем");
                _stack.Push(Math.Sqrt(arg));
                break;
            case "ЭКСП":
                _stack.Push(Math.Exp(arg));
                break;
            case "ЛОГ":
                if (arg <= 0) throw new Exception("Логарифм от неположительного числа");
                _stack.Push(Math.Log(arg));
                break;
            case "СИН":
                _stack.Push(Math.Sin(arg));
                break;
            case "КОС":
                _stack.Push(Math.Cos(arg));
                break;
            case "ТАН":
                _stack.Push(Math.Tan(arg));
                break;
            default:
                throw new Exception($"Неизвестная функция: {func}");
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
                var index = (int)_stack.Pop();
                var arrayIndex = _pc;
                var arrayName = _program[arrayIndex].Value;
                if (!_arrays.TryGetValue(arrayName, out var array))
                {
                    throw new Exception($"Массив не найден: {arrayName}");
                }
                if (index < 0 || index >= array.GetLength(0))
                {
                    throw new Exception($"Индекс вне границ массива: {index}");
                }
                _stack.Push(array[index, 0]);
                break;
            case "[][]":
                // Обработка индексации двумерного массива
                var colIndex = (int)_stack.Pop();
                var rowIndex = (int)_stack.Pop();
                arrayIndex = _pc - 1;
                var arrayName2D = _program[arrayIndex].Value;
                if (!_arrays.TryGetValue(arrayName2D, out var array2D))
                {
                    throw new Exception($"Массив не найден: {arrayName2D}");
                }
                if (rowIndex < 0 || rowIndex >= array2D.GetLength(0) ||
                    colIndex < 0 || colIndex >= array2D.GetLength(1))
                {
                    throw new Exception($"Индекс вне границ массива: [{rowIndex}, {colIndex}]");
                }
                _stack.Push(array2D[rowIndex, colIndex]);
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