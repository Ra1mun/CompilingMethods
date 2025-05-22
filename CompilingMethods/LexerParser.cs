namespace CompilingMethods;

public class Token
{
    public string Value { get; set; }
    public string Type { get; set; }

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

        while (_position < _input.Length)
        {
            char current = _input[_position];

            if (char.IsWhiteSpace(current))
            {
                SkipWhitespace();
                continue;
            }

            if (char.IsLetter(current))
            {
                var identifier = ReadIdentifier();
                if (Definitions.Keywords.Contains(identifier))
                    tokens.Add(new Token { Value = identifier, Type = identifier });
                else
                    tokens.Add(new Token { Value = identifier, Type = "id" });
            }
            else if (char.IsDigit(current))
            {
                var number = ReadNumber();
                tokens.Add(new Token { Value = number, Type = "number" });
            }
            else if (current == '.' || current == '"')
            {
                throw new Exception($"Неподдерживаемый символ: {current}");
            }
            else if (IsTwoCharOperator(current))
            {
                var op = ReadTwoCharOperator();
                tokens.Add(new Token { Value = op, Type = Definitions.Operators[op] });
            }
            else if (Definitions.Operators.ContainsKey(current.ToString()))
            {
                var op = current.ToString();
                tokens.Add(new Token { Value = op, Type = Definitions.Operators[op] });
                _position++;
            }
            else if (Definitions.SingleCharTokens.Contains(current))
            {
                tokens.Add(new Token { Value = current.ToString(), Type = current.ToString() });
                _position++;
            }
            else
            {
                throw new Exception($"Неизвестный символ: {current}");
            }
        }

        tokens.Add(new Token { Value = "EOF", Type = "EOF" });

        return tokens;
    }

    private bool IsTwoCharOperator(char c)
    {
        if (_position + 1 >= _input.Length)
            return false;

        string twoChars = c.ToString() + _input[_position + 1];
        return Definitions.Operators.ContainsKey(twoChars);
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