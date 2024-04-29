using Microsoft.Data.Sqlite;

namespace VaporNotes.UnitTests.Database;

/// <summary>
/// Sqlite in memory databases can be shared across connections as long as one connection remains open.
/// We keep a hidden connection open for the factory lifetime to make sure this is in our control.
/// </summary>
public class InMemoryDatabaseConnectionFactory : IDatabaseConnectionFactory, IDisposable
{
    private SqliteConnection keepAliveConnection;
    private const string ConnectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

    public InMemoryDatabaseConnectionFactory()
    {
        
        keepAliveConnection = new SqliteConnection(ConnectionString);
        keepAliveConnection.Open();
    }

    public SqliteConnection CreateConnection()
    {
        var c = new SqliteConnection(ConnectionString);
        c.Open();
        return c;
    }

    public void Dispose()
    {
        keepAliveConnection.Close();
        keepAliveConnection.Dispose();
    }
}