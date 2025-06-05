namespace CompilingMethods;

public class Interpreter
{
    public static void Interpret(string fullInput)
    {
        var lexer = new LexerParser(fullInput);
        var tokens = lexer.Tokenize();
        
        var parser = new SyntaxParser(tokens);
        parser.Parse_S();
        
        var ops = parser.GetOps();
        
        var interpreter = new OpsInterpreter(ops);
        interpreter.Execute();
    }
}