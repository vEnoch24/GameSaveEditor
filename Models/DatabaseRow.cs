namespace GameSaveEditor.Models;

public sealed class DatabaseRow
{
    public Dictionary<string, object?> Values { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public object? GetValue(string columnName)
        => Values.TryGetValue(columnName, out var value) ? value : null;
}
