using GameSaveEditor.Models;
using System.Text;

namespace GameSaveEditor.Services
{
    public sealed class DatabaseProbeService
    {
        public DatabaseProbeResult Probe(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The selected database file could not be found.", path);
            var info = new FileInfo(path);
            if (info.Length < 16)
                return new(DatabaseFormatKind.Unknown, null, info.Length, "The file is too small to identify as a SQLite database.");
            using var stream = File.OpenRead(path);
            var header = new byte[16];
            _ = stream.Read(header, 0, header.Length);
            var text = Encoding.ASCII.GetString(header);
            return text.StartsWith("SQLite format 3", StringComparison.Ordinal)
                ? new(DatabaseFormatKind.PlainSQLite, text, info.Length, "SQLite format 3 header detected. No encryption is required to identify the file.")
                : new(DatabaseFormatKind.LikelyEncrypted, null, info.Length, "The standard SQLite header is not present. The file may be encrypted or use a non-standard SQLite format.");
        }
    }
}
