using GameSaveEditor.Models;

namespace GameSaveEditor.Services;


public sealed class BackupService
{
    private readonly AppSettingsService _settings;
    public string BackupDirectory { get; private set; } = string.Empty;
    public bool UsedFallbackDirectory
    {
        get; private set;
    }

    public BackupService(AppSettingsService settings)
    {
        _settings = settings;
        BackupDirectory = ResolveDirectory();
    }

    public async Task<string> CreateBackupAsync(string databasePath)
    {
        if (!File.Exists(databasePath))
            throw new FileNotFoundException("Database file was not found.", databasePath);
        var directory = EnsureWritableBackupDirectory();
        var name = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var backupPath = Path.Combine(directory, $"{name}.backup_{timestamp}{extension}");

        await using var source = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 1024, FileOptions.SequentialScan);
        await using var destination = new FileStream(backupPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);
        await source.CopyToAsync(destination);
        await destination.FlushAsync();
        ApplyRetention(databasePath);
        return backupPath;
    }

    public IReadOnlyList<BackupEntry> ListBackups(string? databasePath = null)
    {
        var directory = EnsureWritableBackupDirectory();
        if (!Directory.Exists(directory))
            return [];
        var files = Directory.EnumerateFiles(directory, "*.backup_*.*", SearchOption.TopDirectoryOnly)
            .Where(p => p.EndsWith(".db", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".sqlite3", StringComparison.OrdinalIgnoreCase));
        var databaseName = databasePath is null ? null : Path.GetFileNameWithoutExtension(databasePath);
        return files.Where(path => databaseName is null || Path.GetFileName(path).StartsWith(databaseName + ".backup_", StringComparison.OrdinalIgnoreCase))
            .Select(path => new BackupEntry(Path.GetFileName(path), path, new FileInfo(path).Length, File.GetLastWriteTime(path)))
            .OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task RestoreBackupAsync(string backupPath, string databasePath)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("The selected backup could not be found.", backupPath);
        var targetDirectory = Path.GetDirectoryName(databasePath) ?? throw new InvalidOperationException("Database directory could not be determined.");
        Directory.CreateDirectory(targetDirectory);
        var temp = Path.Combine(targetDirectory, $".{Path.GetFileName(databasePath)}.restore_{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var source = new FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan))
            await using (var destination = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan))
            {
                await source.CopyToAsync(destination);
                await destination.FlushAsync();
            }
            var walPath = databasePath + "-wal";
            var shmPath = databasePath + "-shm";
            if (File.Exists(walPath))
                File.Delete(walPath);
            if (File.Exists(shmPath))
                File.Delete(shmPath);
            File.Copy(temp, databasePath, true);
        }
        finally { try { if (File.Exists(temp)) File.Delete(temp); } catch { } }
    }

    public void DeleteBackup(string backupPath)
    {
        if (File.Exists(backupPath))
            File.Delete(backupPath);
    }

    public void SetSettings(AppSettings settings)
    {
        _settings.Current.BackupDirectoryMode = settings.BackupDirectoryMode;
        _settings.Current.CustomBackupDirectory = settings.CustomBackupDirectory;
        _settings.Current.BackupRetentionCount = Math.Clamp(settings.BackupRetentionCount, 1, 1000);
        _settings.Current.AutoDeleteOldBackups = settings.AutoDeleteOldBackups;
        _settings.Current.ConfirmBeforeRestore = settings.ConfirmBeforeRestore;
        _settings.Save();
        BackupDirectory = ResolveDirectory();
    }

    private string ResolveDirectory()
    {
        var s = _settings.Current;
        return s.BackupDirectoryMode.Equals("Custom", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(s.CustomBackupDirectory)
            ? s.CustomBackupDirectory!
            : Path.Combine(AppContext.BaseDirectory, "Backups");
    }

    private string EnsureWritableBackupDirectory()
    {
        BackupDirectory = ResolveDirectory();
        try
        {
            Directory.CreateDirectory(BackupDirectory);
            var probe = Path.Combine(BackupDirectory, $".write_test_{Guid.NewGuid():N}");
            using (File.Create(probe))
            {
            }
            File.Delete(probe);
            return BackupDirectory;
        }
        catch (UnauthorizedAccessException)
        {
            UsedFallbackDirectory = true;
            BackupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaveDatabaseExplorer", "Backups");
            Directory.CreateDirectory(BackupDirectory);
            return BackupDirectory;
        }
    }

    private void ApplyRetention(string databasePath)
    {
        if (!_settings.Current.AutoDeleteOldBackups)
            return;
        var backups = ListBackups(databasePath).Skip(_settings.Current.BackupRetentionCount).ToList();
        foreach (var backup in backups)
            try
            {
                File.Delete(backup.FullPath);
            }
            catch { }
    }
}

public sealed record BackupEntry(string FileName, string FullPath, long SizeBytes, DateTime CreatedAt);
