using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaporNotes.UnitTests.Database;

namespace VaporNotes.UnitTests;

public class SqlitePrototypeTests : IDisposable
{
    private IDatabaseConnectionFactory connectionFactory;

    public SqlitePrototypeTests()
    {
        connectionFactory = new InMemoryDatabaseConnectionFactory();
    }

    public void Dispose()
    {
        (connectionFactory as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task Streaming()
    {
        //https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/blob-io
        
        //Write
        var fileId = Guid.NewGuid().ToString();
        {
            using var testData = new MemoryStream(Enumerable.Repeat((byte)3, 50).ToArray());
            using var conn = connectionFactory.CreateConnection();
            await conn.ExecuteAsync("CREATE TABLE File(Id TEXT NOT NULL PRIMARY KEY, Data BLOB)");
            var rowId = await conn.ExecuteScalarAsync<long>(@"INSERT INTO File(Id, Data) VALUES (@id, zeroblob(@length));SELECT last_insert_rowid();", new { id = fileId, length = 50 });
            using var blob = new SqliteBlob(conn, "File", "Data", rowId, readOnly: false);
            await testData.CopyToAsync(blob);
        }

        //Read back from a different connection
        {
            using var conn = connectionFactory.CreateConnection();
            var row = await conn.QueryFirstAsync<FileRow>("select rowid as RowId, length(Data) as DataLength from File where Id = @id", new { id = fileId });
            Assert.NotNull(row);
            using var blob = new SqliteBlob(conn, "File", "Data", row.RowId, readOnly: true);
            var buffer = new byte[50];
            await blob.ReadExactlyAsync(buffer);
            Assert.True(buffer.All(x => x == 3));            
        }
    }

    private record FileRow(long RowId, long DataLength);
}
