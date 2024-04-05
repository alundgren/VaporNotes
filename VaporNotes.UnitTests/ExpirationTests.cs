using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests;

public class ExpirationTests
{
    [Fact]
    public async Task NonExpiredNote_IsReturnedByGetNotes()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var service = new VaporNotesService(dropbox, clock, noteDuration: TimeSpan.FromMinutes(2));

        await service.AddNoteAsync("test");
        clock.LetTimePass(TimeSpan.FromMinutes(1));
        var notes = await service.GetNotesAsync();

        Assert.Single(notes);
    }

    [Fact]
    public async Task ExpiredNote_IsNotReturnedByGetNotes()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var service = new VaporNotesService(dropbox, clock, noteDuration: TimeSpan.FromMinutes(2));

        await service.AddNoteAsync("test");
        clock.LetTimePass(TimeSpan.FromMinutes(3));
        var notes = await service.GetNotesAsync();

        Assert.Empty(notes);
    }
}