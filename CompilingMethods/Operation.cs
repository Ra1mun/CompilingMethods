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
    Assign,
    Function,
    ArrayDecl,
    ArraySize,
    String,
    ArrayAssign,
    Array2DAssign,
    Array1DDecl,
    ArrayVariable,
    Array2DVariable
}

public class Operation
{
    public OperationType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Position { get; }

    public Operation(OperationType type, string value, int line, int position)
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