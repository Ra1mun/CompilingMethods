using CompilingMethods;

const int MAX_FILE_SIZE = 1024 * 1024;
var fileName = Console.ReadLine();;

try
{
    if (!File.Exists(fileName))
    {
        throw new FileNotFoundException($"Файл не найден: {fileName}");
    }

    var fullInput = File.ReadAllText(fileName);
    Interpreter.Interpret(fullInput);
}
catch (Exception ex)
{
    Console.WriteLine("\nОшибка: " + ex.Message);
    Environment.Exit(1);
}

