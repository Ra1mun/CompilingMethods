﻿namespace CompilingMethods;

public class Token
{
    public string Value { get; set; }
    public string Type { get; set; }
    public int Line { get; set; }
    public int Position { get; set; }

    public override string ToString()
    {
        return $"[{Type}] {Value}";
    }
}

public class LexerParser
{
    private readonly string _input;
    private int _position;

    public LexerParser(string input)
    {
        _input = input;
        _position = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        int line = 1;
        int position = 0;

        while (_position < _input.Length)
        {
            char current = _input[_position];
            if (current == '\n')
            {
                line++;
                position = 0;
            }

            if (char.IsWhiteSpace(current))
            {
                SkipWhitespace();
                continue;
            }

            if (char.IsLetter(current))
            {
                var identifier = ReadIdentifier();
                if (Definitions.Keywords.Contains(identifier))
                    tokens.Add(new Token { Value = identifier, Type = identifier, Line = line, Position = position });
                else
                    tokens.Add(new Token { Value = identifier, Type = "id", Line = line, Position = position });
            }
            else if (char.IsDigit(current))
            {
                var number = ReadNumber();
                tokens.Add(new Token { Value = number, Type = "number", Line = line, Position = position });
            }
            else if (current == '"')
            {
                var str = ReadString();
                tokens.Add(new Token { Value = str, Type = "string", Line = line, Position = position });
            }
            else if (current == '\'')
            {
                throw new Exception($"Неподдерживаемый символ: {current}");
            }
            else if (IsTwoCharOperator(current))
            {
                var op = ReadTwoCharOperator();
                tokens.Add(new Token { Value = op, Type = op, Line = line, Position = position });
            }
            else if (Definitions.Operators.Contains(current.ToString()))
            {
                var op = current.ToString();
                tokens.Add(new Token { Value = op, Type = op, Line = line, Position = position });
                _position++;
            }
            else if (Definitions.SingleCharTokens.Contains(current))
            {
                tokens.Add(new Token { Value = current.ToString(), Type = current.ToString(), Line = line, Position = position });
                _position++;
            }
            else
            {
                throw new Exception($"Неизвестный символ: {current}");
            }

            position++;
        }

        tokens.Add(new Token { Value = "EOF", Type = "EOF" });

        return tokens;
    }

    private string ReadString()
    {
        _position++; // Пропускаем открывающую кавычку
        int start = _position;
        while (_position < _input.Length && _input[_position] != '"')
        {
            if (_input[_position] == '\\' && _position + 1 < _input.Length && _input[_position + 1] == 'n')
            {
                _position += 2; // Пропускаем \n
                continue;
            }
            if (_input[_position] == '\n')
            {
                throw new Exception("Незакрытая строка");
            }
            _position++;
        }
        
        if (_position >= _input.Length)
        {
            throw new Exception("Незакрытая строка");
        }
        
        string result = _input.Substring(start, _position - start);
        result = result.Replace("\\n", "\n"); // Заменяем \n на реальный перенос строки
        _position++; // Пропускаем закрывающую кавычку
        return result;
    }

    private bool IsTwoCharOperator(char c)
    {
        if (_position + 1 >= _input.Length)
            return false;

        var twoChars = c.ToString() + _input[_position + 1];
        return Definitions.Operators.Contains(twoChars);
    }

    private string ReadTwoCharOperator()
    {
        string result = _input.Substring(_position, 2);
        _position += 2;
        return result;
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
            _position++;
    }

    private string ReadIdentifier()
    {
        int start = _position;
        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            _position++;

        return _input.Substring(start, _position - start);
    }

    private string ReadNumber()
    {
        int start = _position;
        while (_position < _input.Length && char.IsDigit(_input[_position]))
            _position++;

        return _input.Substring(start, _position - start);
    }
}