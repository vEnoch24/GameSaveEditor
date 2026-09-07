namespace GameSaveEditor.Models
{
    public sealed class DatabaseForeignKey
    {
        public int Id
        {
            get; init;
        }
        public string FromColumn { get; init; } = string.Empty;
        public string ReferencedTable { get; init; } = string.Empty;
        public string ReferencedColumn { get; init; } = string.Empty;
        public string OnUpdate { get; init; } = string.Empty;
        public string OnDelete { get; init; } = string.Empty;
    }
}
