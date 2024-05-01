using Dapper;
using Microsoft.Data.Sqlite;

namespace VaporNotes.Api.Database;

/// <summary>
/// Sqlite in memory databases can be shared across connections as long as one connection remains open.
/// We keep a hidden connection open for the factory lifetime to make sure this is in our control.
/// </summary>
public class InMemoryDatabaseConnectionFactory : IDatabaseConnectionFactory, IDisposable
{
    private string ConnectionString => $"Data Source={dbName};Mode=Memory;Cache=Shared";
    private string dbName;
    private SqliteConnection keepAliveConnection;
   

    public InMemoryDatabaseConnectionFactory(ILogger? logger)
    {
        logger?.LogInformation("Using in memory database");
        dbName = Guid.NewGuid().ToString();
        keepAliveConnection = new SqliteConnection(ConnectionString);
        keepAliveConnection.Open();
        CreateTables(keepAliveConnection);
    }

    public SqliteConnection CreateConnection()
    {
        var c = new SqliteConnection(ConnectionString);
        c.Open();
        return c;
    }

    public void Dispose()
    {
        keepAliveConnection.Dispose();
    }

    private static void CreateTables(SqliteConnection conn)
    {
        conn.Execute("CREATE TABLE File(Id TEXT NOT NULL PRIMARY KEY, ContentType TEXT NOT NULL, FileName TEXT NOT NULL, FileData BLOB)");
    }
}