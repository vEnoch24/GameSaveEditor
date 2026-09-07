using GameSaveEditor.Services;

namespace GameSaveEditor.Models
{
    public sealed class DatabaseProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Encryption { get; set; } = "Plain SQLite";
        public string Description { get; set; } = string.Empty;
        public int PageSize { get; set; } = 4096;
        public int KdfIterations { get; set; } = 256000;
        public string KdfAlgorithm { get; set; } = "PBKDF2-HMAC-SHA512";
        public string HmacAlgorithm { get; set; } = "HMAC-SHA512";
        public bool UsesPassphrase
        {
            get; set;
        }
        public bool IsBuiltIn
        {
            get; set;
        }
        public string? DatabasePath
        {
            get; set;
        }
        public DateTime? LastUsedUtc
        {
            get; set;
        }

        public DatabaseProfile Clone() => new()
        {
            Id = Id,
            Name = Name,
            Encryption = Encryption,
            Description = Description,
            PageSize = PageSize,
            KdfIterations = KdfIterations,
            KdfAlgorithm = KdfAlgorithm,
            HmacAlgorithm = HmacAlgorithm,
            UsesPassphrase = UsesPassphrase,
            IsBuiltIn = IsBuiltIn,
            DatabasePath = DatabasePath,
            LastUsedUtc = LastUsedUtc
        };

        public static IReadOnlyList<DatabaseProfile> BuiltIns =>
        [
            new() { Id="plain", Name="Plain SQLite", Encryption="Plain SQLite", Description="Standard unencrypted SQLite database.", UsesPassphrase=false, IsBuiltIn=true },
        new() { Id="solo-leveling", Name="Solo Leveling: ARISE OVERDRIVE", Encryption="SQLCipher 4", Description="Built-in SQLCipher 4 profile for the default Solo Leveling save location. Enter the key when prompted.", UsesPassphrase=true, IsBuiltIn=true, DatabasePath=DatabaseConfiguration.DefaultDatabasePath },
        new() { Id="sqlcipher4", Name="SQLCipher 4", Encryption="SQLCipher", Description="SQLCipher 4 defaults: AES-256-CBC, 4096-byte pages, PBKDF2-HMAC-SHA512.", UsesPassphrase=true, IsBuiltIn=true },
        new() { Id="sqlcipher3", Name="SQLCipher 3 (profile only)", Encryption="SQLCipher", Description="Legacy SQLCipher 3 settings. This build does not ship a SQLCipher 3 native provider, so the profile is available for configuration but cannot currently open a database.", PageSize=1024, KdfIterations=64000, KdfAlgorithm="PBKDF2-HMAC-SHA1", HmacAlgorithm="HMAC-SHA1", UsesPassphrase=true, IsBuiltIn=true },
        new() { Id="sqlcipher-custom", Name="SQLCipher Custom (profile only)", Encryption="SQLCipher", Description="Custom SQLCipher compatibility settings. Native provider support depends on the provider shipped with the application.", UsesPassphrase=true, IsBuiltIn=true }
        ];
    }
}
