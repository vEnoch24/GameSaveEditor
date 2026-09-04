namespace GameSaveEditor.Models;

public sealed class DatabaseColumn
{
    public int Index
    {
        get; init;
    }
    public string Name { get; init; } = string.Empty;
    public string DataType { get; init; } = "ANY";
    public bool IsNotNull
    {
        get; init;
    }
    public bool IsPrimaryKey
    {
        get; init;
    }
    public string? DefaultValue
    {
        get; init;
    }
}
