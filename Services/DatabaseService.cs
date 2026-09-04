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
    public ObservableCollection<DatabaseTable> Tables { get; } = [];
    public DatabaseTable? SelectedTable
    {
        get; private set;
    }

    public async Task OpenAsync(string path, string? password, bool readOnly = false)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("The selected database file could not be found.", path);

        // Ensure the SQLCipher encryption provider is initialized
        SQLitePCL.Batteries_V2.Init();

        await Task.Run(() =>
        {
            Close();

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite,
                Cache = SqliteCacheMode.Private
            };

            if (!string.IsNullOrEmpty(password))
                builder.Password = password; // SQLCipher bundle makes this work natively now

            _connection = new SqliteConnection(builder.ToString());
            _connection.Open();

            // This test query verifies if the password successfully decrypted the file
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT count(*) FROM sqlite_master;";
            _ = command.ExecuteScalar();
        });

        DatabasePath = path;
        ActiveKey = password;
        IsEncryptedConnection = !string.IsNullOrEmpty(password);

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

    public async Task UpdateCellAsync(DatabaseTable table, DatabaseRow row, DatabaseColumn column, object? value)
    {
        EnsureOpen();
        if (!table.HasRowId || row.RowId is null)
            throw new InvalidOperationException("This table has no safe SQLite rowid for editing.");
        await Task.Run(() => { using var cmd = _connection!.CreateCommand(); cmd.CommandText = $"UPDATE {QuoteIdentifier(table.Name)} SET {QuoteIdentifier(column.Name)}=$value WHERE rowid=$rowid;"; cmd.Parameters.AddWithValue("$value", value ?? DBNull.Value); cmd.Parameters.AddWithValue("$rowid", row.RowId.Value); if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("SQLite did not update exactly one row."); });
        row.Values[column.Name] = value;
    }

    public async Task DeleteRowAsync(DatabaseTable table, DatabaseRow row)
    {
        EnsureOpen();
        if (!table.HasRowId || row.RowId is null)
            throw new InvalidOperationException("This table has no safe SQLite rowid for deletion.");
        await Task.Run(() => { using var cmd = _connection!.CreateCommand(); cmd.CommandText = $"DELETE FROM {QuoteIdentifier(table.Name)} WHERE rowid=$rowid;"; cmd.Parameters.AddWithValue("$rowid", row.RowId.Value); if (cmd.ExecuteNonQuery() != 1) throw new InvalidOperationException("The row was not deleted."); });
        table.Rows.Remove(row);
        table.RowCount = Math.Max(0, table.RowCount - 1);
    }

    public async Task<DatabaseRow> InsertRowAsync(DatabaseTable table, IReadOnlyDictionary<string, object?> values)
    {
        EnsureOpen();
        var columns = table.Columns.Where(c => values.ContainsKey(c.Name)).ToList();
        if (columns.Count == 0)
            throw new InvalidOperationException("There are no values to insert.");
        DatabaseRow result = new();
        await Task.Run(() =>
        {
            using var transaction = _connection!.BeginTransaction();
            using var cmd = _connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"INSERT INTO {QuoteIdentifier(table.Name)} ({string.Join(", ", columns.Select(c => QuoteIdentifier(c.Name)))}) VALUES ({string.Join(", ", columns.Select((_, i) => $"$p{i}"))});";
            for (var i = 0; i < columns.Count; i++)
            {
                values.TryGetValue(columns[i].Name, out var v);
                cmd.Parameters.AddWithValue($"$p{i}", v ?? DBNull.Value);
            }
            cmd.ExecuteNonQuery();

            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT last_insert_rowid();";
            var rowId = Convert.ToInt64(command.ExecuteScalar());
            transaction.Commit();
            result = new DatabaseRow { RowId = table.HasRowId ? rowId : null };
            foreach (var c in table.Columns)
                result.Values[c.Name] = values.TryGetValue(c.Name, out var v) ? v : null;
        });
        table.Rows.Add(result);
        table.RowCount++;
        return result;
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
    }
    public void Dispose() => Close();
}
