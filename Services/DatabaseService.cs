using GameSaveEditor.Models;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace GameSaveEditor.Services;

public sealed class DatabaseService : IDisposable
{
    private SqliteConnection? _connection;
    public bool IsOpen => _connection is not null;
    public string DatabasePath { get; private set; } = string.Empty;
    public string? ActiveKey
    {
        get; private set;
    }
    public bool IsEncryptedConnection
    {
        get; private set;
    }
    public DatabaseProfile? ActiveProfile
    {
        get; private set;
    }
    public ObservableCollection<DatabaseTable> Tables { get; } = [];
    public DatabaseTable? SelectedTable
    {
        get; private set;
    }

    public async Task<bool> TestConnectionAsync(string path, string? password, DatabaseProfile? profile = null)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The selected database file could not be found.", path);
        profile ??= string.IsNullOrEmpty(password) ? DatabaseProfile.BuiltIns.First(x => x.Id == "plain") : DatabaseProfile.BuiltIns.First(x => x.Id == "sqlcipher4");
        return profile.Id is "sqlcipher3" or "sqlcipher-custom"
            ? throw new NotSupportedException($"The '{profile.Name}' profile is not supported by the bundled SQLCipher 4 provider.")
            : await Task.Run(() =>
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Cache = SqliteCacheMode.Private };
            if (!string.IsNullOrEmpty(password))
                builder.Password = password;
            using var connection = new SqliteConnection(builder.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM sqlite_master;";
            _ = command.ExecuteScalar();
            return true;
        });
    }

    public async Task OpenAsync(string path, string? password, DatabaseProfile? profile = null, bool readOnly = false)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The selected database file could not be found.", path);
        profile ??= string.IsNullOrEmpty(password) ? DatabaseProfile.BuiltIns.First(x => x.Id == "plain") : DatabaseProfile.BuiltIns.First(x => x.Id == "sqlcipher4");
        if (profile.Id is "sqlcipher3" or "sqlcipher-custom")
            throw new NotSupportedException($"The '{profile.Name}' profile is defined for the editor, but this build uses the bundled SQLCipher 4 native provider. Use SQLCipher 4 or rebuild with a compatible native provider for this profile.");
        await Task.Run(() =>
        {
            Close();
            var builder = new SqliteConnectionStringBuilder { DataSource = path, Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite, Cache = SqliteCacheMode.Private };
            if (!string.IsNullOrEmpty(password))
                builder.Password = password;
            _connection = new SqliteConnection(builder.ToString());
            _connection.Open();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM sqlite_master;";
            _ = command.ExecuteScalar();
        });
        DatabasePath = path;
        ActiveKey = password;
        IsEncryptedConnection = !string.IsNullOrEmpty(password);
        ActiveProfile = profile;
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
            command.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name COLLATE NOCASE;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var sql = reader.IsDBNull(1) ? null : reader.GetString(1);
                var columns = GetColumns(name);
                var table = new DatabaseTable { Name = name, Sql = sql, HasRowId = !IsWithoutRowId(sql) };
                table.Columns.AddRange(columns);
                table.ForeignKeys.AddRange(GetForeignKeys(name));
                result.Add(table);
            }
            foreach (var table in result)
                table.RowCount = GetRowCount(table.Name);
            return result;
        });
        Tables.Clear();
        foreach (var table in tables)
            Tables.Add(table);
        if (SelectedTable is not null)
            SelectedTable = Tables.FirstOrDefault(x => string.Equals(x.Name, SelectedTable.Name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SelectTableAsync(DatabaseTable table)
    {
        await Task.Run(() =>
        {
            EnsureOpen();
            table.Rows.Clear();
            var sql = table.HasRowId ? $"SELECT rowid AS __sde_rowid, * FROM {QuoteIdentifier(table.Name)} LIMIT $limit;" : $"SELECT * FROM {QuoteIdentifier(table.Name)} LIMIT $limit;";
            using var command = _connection!.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$limit", DatabaseConfiguration.MaxRowsPerTable);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                long? rowId = null;
                var start = 0;
                if (table.HasRowId && reader.GetName(0) == "__sde_rowid")
                {
                    rowId = reader.GetInt64(0);
                    start = 1;
                }
                var row = new DatabaseRow { RowId = rowId };
                for (var i = start; i < reader.FieldCount; i++)
                    row.Values[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                table.Rows.Add(row);
            }
        });
        SelectedTable = table;
    }

    public async Task ReloadAsync()
    {
        var name = SelectedTable?.Name;
        await LoadTablesAsync();
        if (name is not null)
        {
            var table = Tables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (table is not null)
                await SelectTableAsync(table);
        }
    }

    public async Task ApplyChangesAsync(DatabaseTable table, PendingChanges changes)
    {
        EnsureOpen();
        if (!table.HasRowId)
            throw new InvalidOperationException("This table has no safe SQLite rowid for editing.");
        if (!changes.HasChanges)
            return;

        await Task.Run(() =>
        {
            using var transaction = _connection!.BeginTransaction();
            try
            {
                foreach (var change in changes.Updates)
                {
                    if (change.Row.RowId is null)
                        throw new InvalidOperationException("An edited row no longer has a valid rowid.");
                    using var cmd = _connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = $"UPDATE {QuoteIdentifier(table.Name)} SET {QuoteIdentifier(change.Column.Name)}=$value WHERE rowid=$rowid;";
                    cmd.Parameters.AddWithValue("$value", change.NewValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("$rowid", change.Row.RowId.Value);
                    if (cmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException($"SQLite did not update row {change.Row.RowId.Value}.");
                }

                foreach (var insert in changes.Inserts)
                {
                    var columns = table.Columns.Where(c => insert.Values.ContainsKey(c.Name)).ToList();
                    if (columns.Count == 0)
                        throw new InvalidOperationException("An inserted row contains no values.");
                    using var cmd = _connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = $"INSERT INTO {QuoteIdentifier(table.Name)} ({string.Join(", ", columns.Select(c => QuoteIdentifier(c.Name)))}) VALUES ({string.Join(", ", columns.Select((_, i) => $"$p{i}"))});";
                    for (var i = 0; i < columns.Count; i++)
                        cmd.Parameters.AddWithValue($"$p{i}", insert.Values[columns[i].Name] ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    using var command = _connection.CreateCommand();
                    command.CommandText = "SELECT last_insert_rowid();";
                    var newRowId = Convert.ToInt64(command.ExecuteScalar());
                    insert.Row.Values.Clear();
                    foreach (var column in table.Columns)
                        insert.Row.Values[column.Name] = insert.Values.GetValueOrDefault(column.Name);
                    insert.RowIdHolder = newRowId;
                }

                foreach (var delete in changes.Deletes)
                {
                    using var cmd = _connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = $"DELETE FROM {QuoteIdentifier(table.Name)} WHERE rowid=$rowid;";
                    cmd.Parameters.AddWithValue("$rowid", delete.RowId);
                    if (cmd.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException($"SQLite did not delete row {delete.RowId}.");
                }
                transaction.Commit();
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch { }
                throw;
            }
        });
    }

    public async Task<QueryResult> ExecuteQueryAsync(string sql)
    {
        EnsureOpen();
        return string.IsNullOrWhiteSpace(sql)
            ? throw new InvalidOperationException("Enter a SQL statement.")
            : await Task.Run(() =>
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            var result = new QueryResult();
            for (var i = 0; i < reader.FieldCount; i++)
                result.Columns.Add(reader.GetName(i));
            while (reader.Read() && result.Rows.Count < DatabaseConfiguration.MaxQueryRows)
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < reader.FieldCount; i++)
                    row[result.Columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                result.Rows.Add(row);
            }
            return result;
        });
    }

    public void CheckpointForBackup()
    {
        EnsureOpen();
        using var command = _connection!.CreateCommand();
        command.CommandText = "PRAGMA wal_checkpoint(FULL);";
        try
        {
            command.ExecuteNonQuery();
        }
        catch (SqliteException) { }
    }

    private List<DatabaseColumn> GetColumns(string tableName)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)});";
        using var reader = cmd.ExecuteReader();
        var list = new List<DatabaseColumn>();
        while (reader.Read())
            list.Add(new DatabaseColumn { Index = reader.GetInt32(0), Name = reader.GetString(1), DataType = reader.IsDBNull(2) ? "ANY" : reader.GetString(2), IsNotNull = reader.GetInt32(3) != 0, DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4), IsPrimaryKey = reader.GetInt32(5) != 0 });
        return list;
    }

    private List<DatabaseForeignKey> GetForeignKeys(string tableName)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = $"PRAGMA foreign_key_list({QuoteIdentifier(tableName)});";
        using var reader = cmd.ExecuteReader();
        var list = new List<DatabaseForeignKey>();
        while (reader.Read())
        {
            list.Add(new DatabaseForeignKey
            {
                Id = reader.GetInt32(0),
                ReferencedTable = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                FromColumn = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                ReferencedColumn = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                OnUpdate = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                OnDelete = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }
        return list;
    }
    private long GetRowCount(string name)
    {
        using var cmd = _connection!.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM {QuoteIdentifier(name)};";
        return Convert.ToInt64(cmd.ExecuteScalar());
    }
    private static bool IsWithoutRowId(string? sql) => sql?.Contains("WITHOUT ROWID", StringComparison.OrdinalIgnoreCase) == true;
    private static string QuoteIdentifier(string id) => "\"" + id.Replace("\"", "\"\"") + "\"";
    private void EnsureOpen()
    {
        if (_connection is null)
            throw new InvalidOperationException("No database is currently open.");
    }
    public void Close()
    {
        SelectedTable = null;
        Tables.Clear();
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        DatabasePath = string.Empty;
        ActiveKey = null;
        IsEncryptedConnection = false;
        ActiveProfile = null;
    }
    public void Dispose() => Close();
}
