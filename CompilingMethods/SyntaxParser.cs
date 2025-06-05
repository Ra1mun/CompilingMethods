namespace CompilingMethods;

using System;
using System.Collections.Generic;

public class SyntaxParser
{
    private readonly List<Token> _tokens;
    private int _position;
    private int _labelCounter;
    private readonly List<Operation> _output = [];
    
    private Token Current => _tokens[_position];

    public SyntaxParser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
        _labelCounter = 0;
    }

    public void Parse_S()
    {
        if (Current.Type == "ЦЕЛ" || Current.Type == "ВЕЩ")
        {
            Match(Current.Type);
            Parse_I();
            Parse_S();
        }
        else
        {
            Parse_Y();
        }
    }

    private void Parse_I()
    {
        Match("id");
        Parse_H();
        Parse_W();
        Match(";");
    }

    private void Parse_W()
    {
        if (Current.Type == ",")
        {
            Match(",");
            Match("id");
            Parse_H();
            Parse_W();
        }
        // else: ε
    }

    private void Parse_Y()
    {
        if (Current.Type == "id")
        {
            var id = Current.Value;
            Match("id");
            if (Current.Type == "[")
            {
                // Это операция с массивом
                Match("[");
                Parse_U(); // Вычисляем индекс
                Match("]");
                if (Current.Type == "=")
                {
                    Match("=");
                    Parse_U(); // Вычисляем значение для присваивания
                    _output.Add(new Operation(OperationType.ArrayAssign, id, Current.Line, Current.Position));
                }
                else
                {
                    _output.Add(new Operation(OperationType.Operator, "[]", Current.Line, Current.Position));
                }
            }
            else
            {
                // Это обычное присваивание
                Match("=");
                Parse_U();
                _output.Add(new Operation(OperationType.Assign, id, Current.Line, Current.Position));
            }
            Match(";");
            Parse_Y();
        }
        else if (Current.Type == "ЕСЛИ")
        {
            var startLabel = $"m{_labelCounter++}";
            var endLabel = $"m{_labelCounter++}";
            
            _output.Add(new Operation(OperationType.Label, startLabel, Current.Line, Current.Position));
            Match("ЕСЛИ");
            Match("(");
            Parse_C();
            _output.Add(new Operation(OperationType.JumpIfFalse, endLabel, Current.Line, Current.Position));
            Match(")");
            Match("{");
            Parse_Y();
            Parse_EY();
            _output.Add(new Operation(OperationType.Label, endLabel, Current.Line, Current.Position));
            Match("}");
            Parse_Y();
        }
        else if (Current.Type == "ПОКА")
        {
            var startLabel = $"m{_labelCounter++}";
            var endLabel = $"m{_labelCounter++}";
            
            _output.Add(new Operation(OperationType.Label, startLabel, Current.Line, Current.Position));
            Match("ПОКА");
            Match("(");
            Parse_C();
            _output.Add(new Operation(OperationType.JumpIfFalse, endLabel, Current.Line, Current.Position));
            Match(")");
            Match("{");
            Parse_Y();
            _output.Add(new Operation(OperationType.Jump, startLabel, Current.Line, Current.Position));
            _output.Add(new Operation(OperationType.Label, endLabel, Current.Line, Current.Position));
            Match("}");
            Parse_Y();
        }
        else if (Current.Type == "ВВОД")
        {
            Match("ВВОД");
            Match("(");
            var id = Current.Value;
            Match("id");
            Parse_H();
            Match(")");
            _output.Add(new Operation(OperationType.Read, id, Current.Line, Current.Position));
            Match(";");
            Parse_Y();
        }
        else if (Current.Type == "ВЫВОД")
        {
            Match("ВЫВОД");
            Match("(");
            Parse_U();
            _output.Add(new Operation(OperationType.Print, "print", Current.Line, Current.Position));
            Match(")");
            Match(";");
            Parse_Y();
        }
        else if (Current.Type == "МАСС")
        {
            Match("МАСС");
            Parse_Array_Decl();
            Parse_Y();
        }
        // else: ε
    }

    private void Parse_EY()
    {
        if (Current.Type == "ИНАЧЕ")
        {
            Match("ИНАЧЕ");
            Match("{");
            Parse_Y();
            Match("}");
        }
        // else: ε
    }

    private void Parse_C()
    {
        Parse_T();
        Parse_U_();
        Parse_K();
    }

    private void Parse_U()
    {
        Parse_T();
        Parse_U_();
    }

    private void Parse_U_()
    {
        if (Current.Type == "+")
        {
            Match("+");
            Parse_T();
            _output.Add(new Operation(OperationType.Operator, "+", Current.Line, Current.Position));
            Parse_U_();
        }
        else if (Current.Type == "-")
        {
            Match("-");
            Parse_T();
            _output.Add(new Operation(OperationType.Operator, "-", Current.Line, Current.Position));
            Parse_U_();
        }
        // else: ε
    }

    private void Parse_T()
    {
        Parse_F();
        Parse_T_();
    }

    private void Parse_T_()
    {
        if (Current.Type == "*")
        {
            Match("*");
            Parse_F();
            _output.Add(new Operation(OperationType.Operator, "*", Current.Line, Current.Position));
            Parse_T_();
        }
        else if (Current.Type == "/")
        {
            Match("/");
            Parse_F();
            _output.Add(new Operation(OperationType.Operator, "/", Current.Line, Current.Position));
            Parse_T_();
        }
        // else: ε
    }

    private void Parse_F()
    {
        if (Current.Type == "~")
        {
            Match("~");
            Parse_R();
            _output.Add(new Operation(OperationType.Operator, "~", Current.Line, Current.Position));
        }
        else
        {
            Parse_R();
        }
    }

    private void Parse_R()
    {
        if (Current.Type == "number")
        {
            _output.Add(new Operation(OperationType.Number, Current.Value, Current.Line, Current.Position));
            Match("number");
        }
        else if (Current.Type == "string")
        {
            _output.Add(new Operation(OperationType.String, Current.Value, Current.Line, Current.Position));
            Match("string");
        }
        else if (Current.Type == "id")
        {
            var id = Current.Value;
            Match("id");
            if (Current.Type == "[")
            {
                Match("[");
                Parse_U(); 
                Match("]");
                _output.Add(new Operation(OperationType.ArrayVariable, id, Current.Line, Current.Position));
            }
            else
                _output.Add(new Operation(OperationType.Variable, id, Current.Line, Current.Position));
            Parse_H();
        }
        else if (Current.Type == "(")
        {
            Match("(");
            Parse_U();
            Match(")");
        }
        else if (Current.Type == "КОР" || Current.Type == "ЭКСП" || Current.Type == "ЛОГ" || 
                 Current.Type == "СИН" || Current.Type == "КОС" || Current.Type == "ТАН")
        {
            var func = Current.Type;
            Match(Current.Type);
            Match("(");
            Parse_U();
            _output.Add(new Operation(OperationType.Function, func, Current.Line, Current.Position));
            Match(")");
        }
    }

    private void Parse_H()
    {
        if (Current.Type == "[")
        {
            Match("[");
            Parse_U(); // Вычисляем первый индекс
            Match("]");
            if (Current.Type == "[")
            {
                Match("[");
                Parse_U(); // Вычисляем второй индекс
                Match("]");
                if (Current.Type == "=")
                {
                    Match("=");
                    Parse_U(); // Вычисляем значение для присваивания
                    _output.Add(new Operation(OperationType.Array2DAssign, "", Current.Line, Current.Position));
                }
                else
                {
                    _output.Add(new Operation(OperationType.Operator, "[][]", Current.Line, Current.Position));
                }
            }
            else
            {
                _output.Add(new Operation(OperationType.Operator, "[]", Current.Line, Current.Position));
            }
        }
    }

    private void Parse_J()
    {
        Parse_Q();
        Parse_J_();
    }

    private void Parse_J_()
    {
        if (Current.Type == "|")
        {
            Match("|");
            Parse_Q();
            _output.Add(new Operation(OperationType.Operator, "|", Current.Line, Current.Position));
            Parse_J_();
        }
        // else: ε
    }

    private void Parse_Q()
    {
        Parse_K();
        Parse_Q_();
    }

    private void Parse_Q_()
    {
        if (Current.Type == "&")
        {
            Match("&");
            Parse_K();
            _output.Add(new Operation(OperationType.Operator, "&", Current.Line, Current.Position));
            Parse_Q_();
        }
        // else: ε
    }

    private void Parse_K()
    {
        if (Current.Type == "(")
        {
            Match("(");
            Parse_U();
            Match(")");
        }
        else
        {
            Parse_U();
            if (Current.Type == ">" || Current.Type == "<" || 
                Current.Type == "#" || Current.Type == "!")
            {
                var op = Current.Type;
                Match(Current.Type);
                Parse_U();
                _output.Add(new Operation(OperationType.Operator, op, Current.Line, Current.Position));
                Parse_Z();
            }
            else
            {
                throw new Exception($"Ошибка в правиле K: ожидается оператор сравнения, найдено '{Current.Value}' на позиции {Current.Line}, {Current.Position}");
            }
        }
    }

    private void Parse_Z()
    {
        // ε - пустое правило
    }

    private void Parse_Array_Decl()
    {
        var arrayName = Current.Value;
        Match("id");
        Match("[");
        Parse_U(); // Вычисляем размер
        _output.Add(new Operation(OperationType.ArraySize, "rows", Current.Line, Current.Position));
        Match("]");
        
        if (Current.Type == "[")
        {
            // Двумерный массив
            Match("[");
            Parse_U(); // Вычисляем размер столбцов
            _output.Add(new Operation(OperationType.ArraySize, "cols", Current.Line, Current.Position));
            Match("]");
            _output.Add(new Operation(OperationType.ArrayDecl, arrayName, Current.Line, Current.Position));
        }
        else
        {
            // Одномерный массив
            _output.Add(new Operation(OperationType.Array1DDecl, arrayName, Current.Line, Current.Position));
        }
        Match(";");
    }

    public List<Operation> GetOps()
    {
        return _output;
    }

    private void Match(string expectedType)
    {
        if (Current.Type == expectedType)
        {
            _position++;
        }
        else
        {
            throw new Exception($"Ожидалось '{expectedType}', найдено '{Current.Type}' на позиции {Current.Line}, {Current.Position}");
        }
    }
}