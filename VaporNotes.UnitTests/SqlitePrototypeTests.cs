using Dapper;
using Microsoft.Data.Sqlite;

namespace VaporNotes.UnitTests;

public class SqlitePrototypeTests : IDisposable
{
    private SqliteConnection connection;

    public SqlitePrototypeTests()
    {
        connection = new SqliteConnection($"Data Source=StreamingPoc;Mode=Memory");
        connection.Open();
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    [Fact]
    public async Task Streaming()
    {
        //https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        
        //Write
        var fileId = Guid.NewGuid().ToString();
        {
            using var testData = new MemoryStream(Enumerable.Repeat((byte)3, 50).ToArray());
            await connection.ExecuteAsync("CREATE TABLE File(Id TEXT NOT NULL PRIMARY KEY, Data BLOB)");
            var rowId = await connection.ExecuteScalarAsync<long>(@"INSERT INTO File(Id, Data) VALUES (@id, zeroblob(@length));SELECT last_insert_rowid();", new { id = fileId, length = 50 });
            using var blob = new SqliteBlob(connection, "File", "Data", rowId, readOnly: false);
            await testData.CopyToAsync(blob);
        }

        //Read back
        {
            var row = await connection.QueryFirstAsync<FileRow>("select rowid as RowId, length(Data) as DataLength from File where Id = @id", new { id = fileId });
            Assert.NotNull(row);
            using var blob = new SqliteBlob(connection, "File", "Data", row.RowId, readOnly: true);
            var buffer = new byte[50];
            await blob.ReadExactlyAsync(buffer);
            Assert.True(buffer.All(x => x == 3));            
        }
    }

    private record FileRow(long RowId, long DataLength);
}
