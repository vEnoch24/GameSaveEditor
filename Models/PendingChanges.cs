namespace GameSaveEditor.Models
{
    public sealed class PendingCellChange
    {
        public required DatabaseRow Row
        {
            get; init;
        }
        public required DatabaseColumn Column
        {
            get; init;
        }
        public object? OriginalValue
        {
            get; init;
        }
        public object? NewValue
        {
            get; set;
        }
    }

    public sealed class PendingInsert
    {
        public required DatabaseRow Row
        {
            get; init;
        }
        public required Dictionary<string, object?> Values
        {
            get; init;
        }
        public long? RowIdHolder
        {
            get => Row.RowId; set => Row.RowId = value;
        }
    }

    public sealed class PendingDelete
    {
        public required DatabaseRow Row
        {
            get; init;
        }
        public required long RowId
        {
            get; init;
        }
    }

    public sealed class PendingChanges
    {
        public List<PendingCellChange> Updates { get; } = [];
        public List<PendingInsert> Inserts { get; } = [];
        public List<PendingDelete> Deletes { get; } = [];

        public bool HasChanges => Updates.Count > 0 || Inserts.Count > 0 || Deletes.Count > 0;
        public int Count => Updates.Count + Inserts.Count + Deletes.Count;

        public void Clear()
        {
            Updates.Clear();
            Inserts.Clear();
            Deletes.Clear();
        }
    }

}
