using Microsoft.Data.Sqlite;

namespace VaporNotes.Api.Database;

public interface IDatabaseConnectionFactory
{
    SqliteConnection CreateConnection();
}