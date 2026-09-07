namespace GameSaveEditor.Models
{
    public sealed class AppSettings
    {
        public int BackupRetentionCount { get; set; } = 30;
        public bool AutoDeleteOldBackups { get; set; } = true;
        public bool ConfirmBeforeRestore { get; set; } = true;
        public string BackupDirectoryMode { get; set; } = "Application";
        public string? CustomBackupDirectory
        {
            get; set;
        }
        public string LastProfileId { get; set; } = "sqlcipher4";
    }
}
