using GameSaveEditor.Models;

using System.Text.Json;

namespace GameSaveEditor.Services
{
    public sealed class AppSettingsService
    {
        private readonly string _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaveDatabaseExplorer");
        private readonly string _path;
        public AppSettings Current { get; private set; } = new();

        public AppSettingsService()
        {
            _path = Path.Combine(_directory, "settings.json");
            Load();
        }

        public void Save()
        {
            Directory.CreateDirectory(_directory);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_path))
                    Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path)) ?? new AppSettings();
            }
            catch { Current = new AppSettings(); }
        }
    }
}
