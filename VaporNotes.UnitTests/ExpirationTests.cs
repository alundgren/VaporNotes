using VaporNotes.UnitTests.Utils;

namespace VaporNotes.UnitTests;

public class ExpirationTests : TestBase
{
    [Fact]
    public async Task NonExpiredNote_IsReturnedByGetNotes()
    {
        var (service, clock) = CreateServices();

        await service.AddNoteAsync("test");
        clock.LetTimePass(TimeSpan.FromMinutes(1));
        var notes = await service.GetNotesAsync();

        Assert.Single(notes);
    }

    [Fact]
    public async Task ExpiredNote_IsNotReturnedByGetNotes()
    {
        var (service, clock) = CreateServices();

        await service.AddNoteAsync("test");
        clock.LetTimePass(TimeSpan.FromMinutes(3));
        var notes = await service.GetNotesAsync();

        Assert.Empty(notes);
    }
}