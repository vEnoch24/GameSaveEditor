using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GameSaveEditor.Services
{
    /// <summary>
    /// Stores database keys encrypted with Windows DPAPI for the current Windows user.
    /// Keys are never stored as plaintext in profiles.json.
    /// </summary>
    public sealed class KeyVaultService
    {
        private readonly string _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaveDatabaseExplorer");
        private readonly string _path;
        private readonly Dictionary<string, string> _keys = new(StringComparer.OrdinalIgnoreCase);

        public KeyVaultService()
        {
            _path = Path.Combine(_directory, "keys.dat");
            Load();
        }

        public bool HasKey(string profileId) => _keys.ContainsKey(profileId);
        public string? GetKey(string profileId) => _keys.TryGetValue(profileId, out var value) ? value : null;

        public void SetKey(string profileId, string key)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                throw new ArgumentException("Profile id is required.", nameof(profileId));
            if (string.IsNullOrEmpty(key))
            {
                RemoveKey(profileId);
                return;
            }
            _keys[profileId] = key;
            Save();
        }

        public void RemoveKey(string profileId)
        {
            if (_keys.Remove(profileId))
                Save();
        }

        public void Clear()
        {
            _keys.Clear();
            try
            {
                if (File.Exists(_path))
                    File.Delete(_path);
            }
            catch { }
        }

        private void Save()
        {
            Directory.CreateDirectory(_directory);
            var json = JsonSerializer.Serialize(_keys);
            var plaintext = Encoding.UTF8.GetBytes(json);
            var protectedBytes = ProtectedData.Protect(plaintext, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(_path, protectedBytes);
            CryptographicOperations.ZeroMemory(plaintext);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return;
                var protectedBytes = File.ReadAllBytes(_path);
                var plaintext = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(plaintext);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (loaded is not null)
                    foreach (var pair in loaded)
                        _keys[pair.Key] = pair.Value;
                CryptographicOperations.ZeroMemory(plaintext);
            }
            catch
            {
                _keys.Clear();
            }
        }
    }
}
