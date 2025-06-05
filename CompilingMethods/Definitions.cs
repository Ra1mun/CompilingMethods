namespace CompilingMethods;

public static class Definitions
{
    // Список ключевых слов
    public static readonly HashSet<string> Keywords = ["ЦЕЛ", "ВЕЩ", "ЕСЛИ", "ИНАЧЕ", "ПОКА", "ВВОД", "ВЫВОД"];

    // Множество разделителей
    public static readonly HashSet<char> SingleCharTokens = ['(', ')', '{', '}', '[', ']', ';', ',', '~'];

    // Множество операций
    public static readonly List<string> Operators = new()
    {
        "+",
        "-",
        "*",
        "/",
        "=",
        "<",
        ">",
        "#", // равно
        "!", // не равно
        "||",
        "&&",
        "~"
    };

    public static readonly Dictionary<int, string> SemanticPrograms = new()
    {
        { 0, "Начало лексемы := C[i]" },
        { 1, "Начало числа:= ord(C[i]) – ord('0')" },
        { 2, "Распознан +" },
        { 3, "Распознан -" },
        { 4, "Распознан *" },
        { 5, "Распознан /" },
        { 6, "Распознан =" },
        { 7, "Распознан <" },
        { 8, "Распознан >" },
        { 9, "Распознан !" },
        { 10, "Распознан ." },
        { 11, "Распознан ," },
        { 12, "Распознан #" },
        { 13, "Распознан (" },
        { 14, "Распознан )" },
        { 15, "Распознан [" },
        { 16, "Распознан ]" },
        { 17, "Распознан {" },
        { 18, "Распознан }" },
        { 19, "Распознан ;" },
        { 20, "Распознан пробел" },
        { 21, "Символа нет в языке" },
        { 22, "Переход на новую строку" },
        { 23, "Конец" },
        { 24, "Продолжение лексемы" },
        { 25, "Распознано имя" },
        { 26, "n := n * 10 + ord(C[i]) – ord('0')" },
        { 27, "Распознано число" },
        { 28, "Ошибка в лексеме" },
        { 29, "d := d * 0.1; x := x + (ord(C[i]) – ord('0')) * d;" },
        { 30, "Распознан унарный минус" },
        { 31, "Распознан |" },
        { 32, "Распознан &" },
        { 33, "Распознано десятичное число" }
    };
    
    public static readonly Dictionary<string, int> Map = new()
    {
        { "=", 16 },
        { "<", 17 },
        { ">", 18 },
        { "#", 19 }, // ==
        { "!=", 22 },
        { "+", 3 },
        { "-", 4 },
        { "*", 5 },
        { "/", 6 },
        { "ВЫВОД", 26 },
        { "ВВОД", 25 },
        { "sqrt", 27 },
        { "exp", 28 },
        { "log_b", 29 },
        { "j", 34 },
        { "jf", 35 },
        { "new", 31 },
        { "ind", 36 }
    };


    public static readonly State[,] TransitionTable = new[,]
    {
        // буква | цифра | + | - | * | / | = | < | > | ! | . | , | # | ( | ) | [ | ] | { | } | ~ | || | & | ; | пробел | др | \n | EOF
        {
            State.A, State.B, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.Z, State.E,
            State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.S,
            State.Z, State.E, State.E
        }, // S
        {
            State.A, State.A, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.Z, State.E,
            State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E,
            State.Z, State.E, State.E
        }, // A
        {
            State.Z, State.B, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.C, State.E,
            State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E,
            State.Z, State.E, State.E
        }, // B
        {
            State.Z, State.D, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z,
            State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z,
            State.Z, State.Z, State.Z
        }, // C
        {
            State.Z, State.D, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.Z, State.E,
            State.E, State.Z, State.E, State.Z, State.E, State.Z, State.E, State.E, State.E, State.E, State.E, State.E,
            State.Z, State.E, State.E
        }, // D
        {
            State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E,
            State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E, State.E,
            State.E, State.E, State.E
        }, // E
        {
            State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z,
            State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z, State.Z,
            State.Z, State.Z, State.Z
        } // Z
    };
}

public enum State
{
    S,
    A,
    B,
    C,
    D,
    E,
    Z
}