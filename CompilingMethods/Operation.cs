namespace CompilingMethods;

public enum OperationType
{
    Number,
    Variable,
    Operator,
    Label,
    Jump,
    JumpIfFalse,
    Print,
    Read,
    Assign
}

public class Operation
{
    public OperationType Type { get; set; }
    public string Value { get; set; }
    public int Line { get; set; }
    public int Position { get; set; }

    public Operation(OperationType type, string value, int line = 0, int position = 0)
    {
        Type = type;
        Value = value;
        Line = line;
        Position = position;
    }

    public override string ToString()
    {
        return Value;
    }
} 