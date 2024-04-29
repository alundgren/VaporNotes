using Microsoft.Data.Sqlite;

namespace VaporNotes.UnitTests.Database;

public interface IDatabaseConnectionFactory
{
    SqliteConnection CreateConnection();
}