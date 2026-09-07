namespace GameSaveEditor.Models
{

    public enum DatabaseFormatKind
    {
        PlainSQLite, LikelyEncrypted, Unknown
    }

    public sealed record DatabaseProbeResult(DatabaseFormatKind Format, string? Header, long SizeBytes, string Summary);
}
