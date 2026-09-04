namespace GameSaveEditor.Services;

public static class DatabaseConfiguration
{
    public const string DefaultDatabasePath =
        @"C:\Users\Enoch\AppData\LocalLow\NetmarbleNeo\Solo_Leveling_ARISE_OVERDRIVE\76561197960271872\SaveFolder\Slot1.db";

    // Treat this as a SQLCipher PASSPHRASE.
    // Do not Base64-decode it before passing it to SQLCipher.
    public const string EncryptionKey =
        "eNYKHF1fzulUnlMLl9Adb8AN5Ar/P/UmD+7x3Rrc/v0=";

    // Phase 1 intentionally caps the number of rows rendered at once.
    public const int MaxRowsPerTable = 5000;
}
