using System.Text;
using VaporNotes.Api.Domain;
using VaporNotes.UnitTests.Utils;

namespace VaporNotes.UnitTests;

public class FileUploadTests : TestBase
{
    [Fact]
    public async Task Upload_WithValidKey_Works()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var store = new PendingUploadStore();
        var service = new VaporNotesService(dropbox, clock, DurationConfig(TimeSpan.FromMinutes(2)), store);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        var notes = await service.GetNotesAsync();
        Assert.NotNull(notes?.FirstOrDefault()?.AttachedDropboxFile);
    }

    [Fact]
    public async Task Upload_WithAlreadyUsedKey_Throws()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var store = new PendingUploadStore();
        var service = new VaporNotesService(dropbox, clock, DurationConfig(TimeSpan.FromMinutes(2)), store);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        async Task Upload() => await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        await Upload();

        await Assert.ThrowsAsync<Exception>(Upload);
    }

    [Fact]
    public async Task Upload_Text_IsFilename()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var store = new PendingUploadStore();
        var service = new VaporNotesService(dropbox, clock, DurationConfig(TimeSpan.FromMinutes(2)), store);

        var uploadKey = await service.CreateSingleUseUploadKeyAsync(new UploadFileMetadata("test.txt"));

        await service.CompleteUploadAsync(uploadKey, new MemoryStream(Encoding.UTF8.GetBytes("ABC")));

        var notes = await service.GetNotesAsync();
        Assert.Equal("test.txt", notes?.FirstOrDefault()?.Text);
    }
}