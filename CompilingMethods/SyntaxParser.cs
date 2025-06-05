namespace CompilingMethods;

using System;
using System.Collections.Generic;

public class SyntaxParser
{
    private readonly List<Token> _tokens;
    private int _position;
    private int _labelCounter;
    private readonly List<Operation> _output = new();
    
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
        var id = Current.Value;
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
            var id = Current.Value;
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
            Parse_H();
            Match("=");
            Parse_U();
            _output.Add(new Operation(OperationType.Assign, id, Current.Line, Current.Position));
            Match(";");
            Parse_Y();
        }
        else if (Current.Type == "ЕСЛИ")
        {
            Match("ЕСЛИ");
            Match("(");
            Parse_C();
            Match(")");
            Match(";");
            Parse_Y();
            Parse_EY();
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
            Parse_H();
        }
        else if (Current.Type == "id")
        {
            _output.Add(new Operation(OperationType.Variable, Current.Value, Current.Line, Current.Position));
            Match("id");
            Parse_H();
        }
        else if (Current.Type == "(")
        {
            Match("(");
            Parse_U();
            Match(")");
        }
    }

    private void Parse_H()
    {
        if (Current.Type == "[")
        {
            Match("[");
            Parse_U();
            _output.Add(new Operation(OperationType.Operator, "[]", Current.Line, Current.Position));
            Match("]");
        }
        // else: ε
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
            throw new Exception($"Ожидалось '{expectedType}', найдено '{Current.Type}'");
        }
    }
}