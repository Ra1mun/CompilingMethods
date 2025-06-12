    using CompilingMethods;

const int MAX_FILE_SIZE = 1024 * 1024; // 1 MB
const string WORK_DIR = @"C:\Users\sobol\Documents\CompilingMethods\CompilingMethods";

Console.WriteLine("Введите имя файла:");
var fileName = Console.ReadLine();

try
{
    // Получаем полный путь к файлу в рабочей директории
    var fullPath = Path.Combine(WORK_DIR, fileName);
    
    if (!File.Exists(fullPath))
    {
        throw new FileNotFoundException($"Файл не найден: {fullPath}");
    }

    var fileInfo = new FileInfo(fullPath);
    if (fileInfo.Length > MAX_FILE_SIZE)
    {
        throw new Exception($"Файл слишком большой. Максимальный размер: {MAX_FILE_SIZE / 1024} KB");
    }

    var fullInput = File.ReadAllText(fullPath);
    Interpreter.Interpret(fullInput);
}
catch (Exception ex)
{
    Console.WriteLine("\nОшибка: " + ex.Message);
    Environment.Exit(1);
}

