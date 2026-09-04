namespace GameSaveEditor.Models;

public sealed class DatabaseTable
{
    public string Name { get; init; } = string.Empty;
    public string? Sql
    {
        get; init;
    }
    public long RowCount
    {
        get; set;
    }
    public bool HasRowId
    {
        get; init;
    }
    public List<DatabaseColumn> Columns { get; } = [];
    public List<DatabaseRow> Rows { get; } = [];
}
