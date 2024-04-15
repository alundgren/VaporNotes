using Microsoft.Extensions.Configuration;
using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests;

public class FileUploadTests
{
    [Fact]
    public async Task Foo()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var service = new VaporNotesService(dropbox, clock, DurationConfig(TimeSpan.FromMinutes(2)));

        var notes = await service.BeginUploadAsync("abc123.txt");
        

        await service.AddNoteAsync("test");
        clock.LetTimePass(TimeSpan.FromMinutes(1));
        var notes = await service.GetNotesAsync();

        Assert.Single(notes);
    }

    private IConfiguration DurationConfig(TimeSpan t) => new FakeConfiguration().Set("VaporNotes:NoteDuration", t.ToString("c"));
}