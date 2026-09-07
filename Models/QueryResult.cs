namespace GameSaveEditor.Models
{
    public sealed class QueryResult
    {
        public List<string> Columns { get; } = [];
        public List<Dictionary<string, object?>> Rows { get; } = [];
        public int AffectedRows
        {
            get; init;
        }
    }
}
