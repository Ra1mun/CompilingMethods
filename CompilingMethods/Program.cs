using CompilingMethods;

Console.WriteLine("Введите программу построчно. Пустая строка — завершение ввода.");
string line;
var fullInput = "";
while (true)
{
    Console.Write("> ");
    line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line)) break;
    fullInput += line + " ";
}

try
{
    var lexer = new LexerParser(fullInput);
    var tokens = lexer.Tokenize();
    Console.WriteLine("\nТокены:");
    Console.WriteLine(string.Join(" ", tokens));

    Console.WriteLine("\nСинтаксический разбор:");
    var parser = new SyntaxParser(tokens);
    parser.Parse_S();
    
    Console.WriteLine("\nАнализ завершён успешно!");
}
catch (Exception ex)
{
    Console.WriteLine("\nОшибка: " + ex.Message);
}

fullInput = null;
while (true)
{
    Console.Write("> ");
    line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line)) break;
    fullInput += line + " ";
}

Console.WriteLine("\nДетерминированный конечный автомат:");
var transitionTable = new TransitionTable();
transitionTable.Parse(fullInput);