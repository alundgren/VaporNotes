using System.Text;
using VaporNotes.Api.Database;
using VaporNotes.Api.Domain;
using VaporNotes.UnitTests.Utils;

namespace VaporNotes.UnitTests;

public class FileUploadTests : TestBase
{
    [Fact]
    public async Task Upload_WithValidKey_Works()
    {
        var clock = new FakeVaporNotesClock();
        var db = new InMemoryDatabaseConnectionFactory(null);
        var store = new PendingUploadStore();
        var service = new VaporNotesService(clock, DurationConfig(TimeSpan.FromMinutes(2)), store, db);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        var notes = await service.GetNotesAsync();
        Assert.NotNull(notes?.FirstOrDefault()?.AttachedFileId);
    }

    [Fact]
    public async Task Upload_WithAlreadyUsedKey_Throws()
    {
        var clock = new FakeVaporNotesClock();
        var db = new InMemoryDatabaseConnectionFactory(null);
        var store = new PendingUploadStore();
        var service = new VaporNotesService(clock, DurationConfig(TimeSpan.FromMinutes(2)), store, db);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        async Task Upload() => await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        await Upload();

        await Assert.ThrowsAsync<Exception>(Upload);
    }

    [Fact]
    public async Task Upload_Text_IsFilename()
    {
        var clock = new FakeVaporNotesClock();
        var db = new InMemoryDatabaseConnectionFactory(null);
        var store = new PendingUploadStore();
        var service = new VaporNotesService(clock, DurationConfig(TimeSpan.FromMinutes(2)), store, db);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        var notes = await service.GetNotesAsync();
        Assert.Equal("test.txt", notes?.FirstOrDefault()?.Text);
    }
}