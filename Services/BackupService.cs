namespace GameSaveEditor.Services;

public sealed class BackupService
{
    public async Task<string> CreateBackupAsync(string databasePath)
    {
        if (!File.Exists(databasePath))
            throw new FileNotFoundException("Database file was not found.", databasePath);

        var directory = Path.GetDirectoryName(databasePath)
                        ?? throw new InvalidOperationException("Database directory could not be determined.");
        var name = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var backupPath = Path.Combine(directory, $"{name}.backup_{timestamp}{extension}");

        await using var source = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024, FileOptions.SequentialScan);
        await using var destination = new FileStream(backupPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);
        await source.CopyToAsync(destination);
        await destination.FlushAsync();
        return backupPath;
    }
}
