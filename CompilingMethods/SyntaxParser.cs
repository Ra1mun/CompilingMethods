namespace CompilingMethods;

using System;
using System.Collections.Generic;

public class SyntaxParser
{
    private readonly List<Token> _tokens;
    private int _position;
    
    private Token Current => _tokens[_position];
    private readonly Stack<Token> _stack = new();

    public SyntaxParser(List<Token> tokens)
    {
        _tokens = tokens;
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
            Match("id");
            Parse_H();
            Match("=");
            Parse_U();
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
            Match("ПОКА");
            Match("(");
            Parse_C();
            Match(")");
            Match("{");
            Parse_Y();
            Match("}");
        }
        else if (Current.Type == "ВВОД")
        {
            Match("ВВОД");
            Match("(");
            Match("id");
            Parse_H();
            Match(")");
            Match(";");
            Parse_Y();
        }
        else if (Current.Type == "ВЫВОД")
        {
            Match("ВЫВОД");
            Match("(");
            Parse_U();
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
            Parse_U_();
        }
        else if (Current.Type == "-")
        {
            Match("-");
            Parse_T();
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
            Parse_T_();
        }
        else if (Current.Type == "/")
        {
            Match("/");
            Parse_F();
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
            Match("number");
            Parse_H();
        }
        else if (Current.Type == "id")
        {
            Match("id");
            Parse_H();
        }
        else if (Current.Type == "(")
        {
            Match("(");
            Parse_U();
            Match(")");
        }
        else
        {
            throw new Exception($"Ошибка в правиле R: ожидается 'number', '(', 'id' найдено '{Current.Value}' на позиции {Current.Line}, {Current.Position}");
        }
    }

    private void Parse_H()
    {
        if (Current.Type == "[")
        {
            Match("[");
            Parse_U();
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
                Match(Current.Type);
                Parse_U();
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

    public Stack<Token> GetOps()
    {
        return _stack;
    }

    private void Match(string expectedType)
    {
        if (Current.Type == expectedType)
        {
            Console.WriteLine($"matched token: {Current.Value} ({Current.Type})");
            _position++;
        }
        else
        {
            throw new Exception($"Ожидалось '{expectedType}', найдено '{Current.Type}'");
        }
    }
}