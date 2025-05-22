namespace CompilingMethods;

using System;
using System.Collections.Generic;

public class SyntaxParser
{
    private readonly List<Token> _tokens;
    private int _position;

    public SyntaxParser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    // === Синтаксические процедуры ===

    public void Parse_S()
    {
        var currentType = Current().Type;

        if (currentType == "ЦЕЛ" || currentType == "ВЕЩ")
        {
            Match(currentType);
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
        if (Current().Type == ",")
        {
            Match(",");
            Parse_I();
        }
        // else: ε
    }

    private void Parse_Y()
    {
        if (Current().Type == "id")
        {
            Parse_I();
            Match("op_assign");
            Parse_U();
            Match(";");
            Parse_Y();
        }
        else if (Current().Type == "ЕСЛИ")
        {
            Match("ЕСЛИ");
            Match("(");
            Parse_C();
            Match(")");
            Match(";");
            Parse_Y();
            Parse_EY();
        }
        else if (Current().Type == "ПОКА")
        {
            Match("ПОКА");
            Match("(");
            Parse_C();
            Match(")");
            Match(";");
            Parse_Y();
        }
        else if (Current().Type == "ВВОД")
        {
            Match("ВВОД");
            Match("(");
            Match("id");
            Match(")");
            Match(";");
            Parse_Y();
        }
        else if (Current().Type == "ВЫВОД")
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
        if (Current().Type == "ИНАЧЕ")
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
        Parse_U();
        Parse_K();
    }

    private void Parse_U()
    {
        Parse_T();
        Parse_U_();
    }

    private void Parse_U_()
    {
        if (Current().Type == "op_add" || Current().Type == "op_sub")
        {
            Match(Current().Type);
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
        if (Current().Type == "op_mul" || Current().Type == "op_div")
        {
            Match(Current().Type);
            Parse_F();
            Parse_T_();
        }
        // else: ε
    }

    private void Parse_F()
    {
        if (Current().Type == "op_unsub")
        {
            Match("op_unsub");
            Parse_R();
        }
        else
        {
            Parse_R();
        }
    }

    private void Parse_R()
    {
        if (Current().Type == "id")
        {
            Match("id");
            Parse_H();
        }
        else if (Current().Type == "(")
        {
            Match("(");
            Parse_U();
            Match(")");
        }
        else
        {
            throw new Exception($"Ошибка в выражении: ожидается id или '(', найдено '{Current().Value}'");
        }
    }

    private void Parse_H()
    {
        if (Current().Type == "[")
        {
            Match("[");
            Parse_U();
            Match("]");
        }
        // else: ε
    }

    private void Parse_K()
    {
        if (Current().Type == "op_less" || Current().Type == "op_greater" || Current().Type == "op_eq" || Current().Type == "op_neq")
        {
            Match(Current().Type);
            Parse_U();
        }
        // else: ε
    }

    // === Лексический анализатор ===

    private Token Current()
    {
        if (_position >= _tokens.Count)
            return new Token { Type = "EOF", Value = "" };
        return _tokens[_position];
    }

    private void Match(string expectedType)
    {
        if (Current().Type == expectedType)
        {
            Console.WriteLine($"matched token: {Current().Value} ({Current().Type})");
            _position++;
        }
        else
        {
            throw new Exception($"Ожидалось '{expectedType}', найдено '{Current().Type}'");
        }
    }
}