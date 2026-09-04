using GameSaveEditor.Models;
using GameSaveEditor.Services;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace GameSaveEditor.Services;

public sealed class DatabaseService : IDisposable
{
    private SqliteConnection? _connection;

    public bool IsOpen => _connection is not null;

    public string DatabasePath { get; private set; } = string.Empty;

    public ObservableCollection<DatabaseTable> Tables { get; } = [];

    public DatabaseTable? SelectedTable { get; private set; }

    public async Task OpenAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("A database path is required.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException(
                "The selected database file could not be found.",
                path);

        await Task.Run(() =>
        {
            Close();

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,

                // This is the exact key/passphrase supplied for the save.
                Password = DatabaseConfiguration.EncryptionKey
            };

            _connection = new SqliteConnection(builder.ToString());
            _connection.Open();

            // Opening the connection does not by itself prove the key is valid.
            // Force SQLite to read the encrypted schema.
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM sqlite_master;";
            _ = command.ExecuteScalar();
        });

        DatabasePath = path;

        await LoadTablesAsync();

        if (Tables.Count > 0)
            await SelectTableAsync(Tables[0]);
    }

    public async Task LoadTablesAsync()
    {
        var tables = await Task.Run(() =>
        {
            EnsureOpen();

            var result = new List<DatabaseTable>();

            using var command = _connection!.CreateCommand();
            command.CommandText =
                """
                SELECT name, sql
                FROM sqlite_master
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                ORDER BY name COLLATE NOCASE;
                """;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var tableName = reader.GetString(0);
                var sql = reader.IsDBNull(1) ? null : reader.GetString(1);

                var table = new DatabaseTable
                {
                    Name = tableName,
                    Sql = sql
                };

                table.Columns.AddRange(GetColumns(tableName));
                table.RowCount = GetRowCount(tableName);

                result.Add(table);
            }

            return result;
        });

        Tables.Clear();

        foreach (var table in tables)
            Tables.Add(table);

        if (SelectedTable is not null)
        {
            var replacement = Tables.FirstOrDefault(
                x => string.Equals(
                    x.Name,
                    SelectedTable.Name,
                    StringComparison.OrdinalIgnoreCase));

            SelectedTable = replacement;
        }
    }

    public async Task SelectTableAsync(DatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        await Task.Run(() =>
        {
            EnsureOpen();

            table.Rows.Clear();

            var quotedTable = QuoteIdentifier(table.Name);

            using var command = _connection!.CreateCommand();
            command.CommandText =
                $"SELECT * FROM {quotedTable} LIMIT $limit;";
            command.Parameters.AddWithValue(
                "$limit",
                DatabaseConfiguration.MaxRowsPerTable);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var row = new DatabaseRow();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i)
                        ? null
                        : reader.GetValue(i);

                    row.Values[reader.GetName(i)] = value;
                }

                table.Rows.Add(row);
            }
        });

        SelectedTable = table;
    }

    public async Task ReloadAsync()
    {
        var selectedName = SelectedTable?.Name;

        await LoadTablesAsync();

        if (selectedName is null)
            return;

        var table = Tables.FirstOrDefault(
            x => string.Equals(
                x.Name,
                selectedName,
                StringComparison.OrdinalIgnoreCase));

        if (table is not null)
            await SelectTableAsync(table);
    }

    private List<DatabaseColumn> GetColumns(string tableName)
    {
        EnsureOpen();

        var columns = new List<DatabaseColumn>();
        var pragmaTarget = QuoteIdentifier(tableName);

        using var command = _connection!.CreateCommand();
        command.CommandText = $"PRAGMA table_info({pragmaTarget});";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            columns.Add(new DatabaseColumn
            {
                Index = reader.GetInt32(0),
                Name = reader.GetString(1),
                DataType = reader.IsDBNull(2)
                    ? "ANY"
                    : reader.GetString(2),
                IsNotNull = reader.GetInt32(3) != 0,
                DefaultValue = reader.IsDBNull(4)
                    ? null
                    : reader.GetString(4),
                IsPrimaryKey = reader.GetInt32(5) != 0
            });
        }

        return columns;
    }

    private long GetRowCount(string tableName)
    {
        EnsureOpen();

        var quotedTable = QuoteIdentifier(tableName);

        using var command = _connection!.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {quotedTable};";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private void EnsureOpen()
    {
        if (_connection is null)
            throw new InvalidOperationException(
                "No database is currently open.");
    }

    private static string QuoteIdentifier(string identifier)
    {
        // SQLite identifier escaping: " becomes "".
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }

    public void Close()
    {
        SelectedTable = null;
        Tables.Clear();

        if (_connection is not null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        DatabasePath = string.Empty;
    }

    public void Dispose() => Close();
}
