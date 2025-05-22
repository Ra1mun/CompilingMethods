namespace CompilingMethods;

public class TransitionTable
{
    public void Parse(string input)
    {
        var current = State.S;
        var action = -1;
        
        foreach (char ch in input)
        {
            var col = CharType(ch);
            var next = Transition(current, col);

            Console.Write($"Символ: '{ch}' | ");
            Console.Write($"Текущее состояние: {current} | ");
            Console.Write($"Следующее состояние: {next}");

            action = GetAction(current, ch);
            if (action != -1 && Definitions.SemanticPrograms.ContainsKey(action))
                Console.Write($" | Действие {action}: {Definitions.SemanticPrograms[action]}");

            Console.WriteLine();

            current = next;

            if (current == State.Z)
            {
                throw new Exception("❌ Ошибка: некорректная лексема");
            }

            if (current == State.E)
                current = State.S; // Возврат в начальное состояние
        }
    }
    
    public State Transition(State current, int col)
    {
        return Definitions.TransitionTable[(int)current, col];
    }
    
    public static int CharType(char ch)
    {
        return ch switch
        {
            _ when char.IsLetter(ch) => 0,      // буква
            _ when char.IsDigit(ch) => 1,       // цифра
            '+' => 2,
            '-' => 3,
            '*' => 4,
            '/' => 5,
            '=' => 6,
            '<' => 7,
            '>' => 8,
            '!' => 9,
            '.' => 10,
            ',' => 11,
            '#' => 12,
            '(' => 13,
            ')' => 14,
            '[' => 15,
            ']' => 16,
            '{' => 17,
            '}' => 18,
            ';' => 19,
            ' ' => 20,
            '\n' => 21,
            '\0' => 22, // EOF
            _ => 23      // другие символы
        };
    }

    private int GetAction(State current, char ch)
    {
        int col = CharType(ch);

        return current switch
        {
            State.S when char.IsLetter(ch) => 0,
            State.S when char.IsDigit(ch) => 1,
            State.S when ch == '+' => 2,
            State.S when ch == '-' => 3,
            State.S when ch == '*' => 4,
            State.S when ch == '/' => 5,
            State.S when ch == '=' => 6,
            State.S when ch == '<' => 7,
            State.S when ch == '>' => 8,
            State.S when ch == '!' => 9,
            State.S when ch == '.' => 10,
            State.S when ch == ',' => 11,
            State.S when ch == '#' => 12,
            State.S when ch == '(' => 13,
            State.S when ch == ')' => 14,
            State.S when ch == '[' => 15,
            State.S when ch == ']' => 16,
            State.S when ch == '{' => 17,
            State.S when ch == '}' => 18,
            State.S when ch == ';' => 19,
            State.S when ch == ' ' => 20,
            State.S when ch == '\n' => 22,
            State.S when ch == '\0' => 23,
            State.A when !char.IsLetterOrDigit(ch) => 25,
            State.B when char.IsDigit(ch) => 26,
            State.B when ch == '+' => 27,
            State.B when ch == '-' => 27,
            State.B when ch == '*' => 27,
            State.B when ch == '/' => 27,
            State.B when ch == '=' => 27,
            State.B when ch == '.' => 10,
            State.C => 29,
            State.D when char.IsDigit(ch) => 29,
            _ => 33
        };
    }
}