using GameSaveEditor.Models;
using System.Text.Json;

namespace GameSaveEditor.Services
{
    public sealed class ProfileService
    {
        private readonly KeyVaultService _keyVault;
        private readonly string _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaveDatabaseExplorer");
        private readonly string _path;
        public List<DatabaseProfile> Profiles { get; } = [];
        private readonly Dictionary<string, string> _sessionKeys = new(StringComparer.OrdinalIgnoreCase);

        public ProfileService(KeyVaultService keyVault)
        {
            _keyVault = keyVault;
            _path = Path.Combine(_directory, "profiles.json");
            Load();
        }

        public IReadOnlyList<DatabaseProfile> All => DatabaseProfile.BuiltIns.Concat(Profiles).ToList();

        public DatabaseProfile? Find(string id) => All.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        public string? GetSessionKey(string profileId) => _sessionKeys.TryGetValue(profileId, out var key) ? key : null;
        public string? GetStoredKey(string profileId) => _keyVault.GetKey(profileId);
        public bool HasStoredKey(string profileId) => _keyVault.HasKey(profileId);
        public void SetStoredKey(string profileId, string key) => _keyVault.SetKey(profileId, key);
        public void RemoveStoredKey(string profileId) => _keyVault.RemoveKey(profileId);
        public void ClearStoredKeys() => _keyVault.Clear();
        public void SetSessionKey(string profileId, string key) => _sessionKeys[profileId] = key;
        public void ClearSessionKeys() => _sessionKeys.Clear();

        public void SaveProfile(DatabaseProfile profile)
        {
            if (profile.IsBuiltIn)
                throw new InvalidOperationException("Built-in profiles cannot be modified.");
            if (string.IsNullOrWhiteSpace(profile.Id))
                profile.Id = "profile_" + Guid.NewGuid().ToString("N");
            var existing = Profiles.FindIndex(p => p.Id.Equals(profile.Id, StringComparison.OrdinalIgnoreCase));
            if (existing >= 0)
                Profiles[existing] = profile.Clone();
            else
                Profiles.Add(profile.Clone());
            Save();
        }

        public void DeleteProfile(string id)
        {
            var profile = Profiles.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (profile is null)
                return;
            Profiles.Remove(profile);
            Save();
        }

        public void MarkUsed(DatabaseProfile profile, string path)
        {
            var stored = Profiles.FirstOrDefault(p => p.Id.Equals(profile.Id, StringComparison.OrdinalIgnoreCase));
            if (stored is not null)
            {
                stored.DatabasePath = path;
                stored.LastUsedUtc = DateTime.UtcNow;
                Save();
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return;
                var loaded = JsonSerializer.Deserialize<List<DatabaseProfile>>(File.ReadAllText(_path));
                if (loaded is not null)
                    Profiles.AddRange(loaded.Where(p => !p.IsBuiltIn));
            }
            catch { Profiles.Clear(); }
        }

        private void Save()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_path, JsonSerializer.Serialize(Profiles, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
