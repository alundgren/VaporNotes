using Microsoft.Extensions.Configuration;
using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests.Utils;

public abstract class TestBase
{
    protected (VaporNotesService Service, FakeVaporNotesClock Clock) CreateServices()
    {
        var clock = new FakeVaporNotesClock();
        var dropbox = new FakeDropboxService();
        var store = new PendingUploadStore();
        return (new VaporNotesService(dropbox, clock, DurationConfig(TimeSpan.FromMinutes(2)), store), clock);
    }

    protected IConfiguration DurationConfig(TimeSpan t) => new FakeConfiguration().Set("VaporNotes:NoteDuration", t.ToString("c"));
}
