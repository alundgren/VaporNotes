using Microsoft.Extensions.Configuration;
using VaporNotes.Api.Database;
using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests.Utils;

public abstract class TestBase
{
    protected (VaporNotesService Service, FakeVaporNotesClock Clock) CreateServices()
    {
        var clock = new FakeVaporNotesClock();
        var db = new InMemoryDatabaseConnectionFactory(null);
        var store = new PendingUploadStore();
        return (new VaporNotesService(clock, DurationConfig(TimeSpan.FromMinutes(2)), store, db), clock);
    }

    protected IConfiguration DurationConfig(TimeSpan t) => new FakeConfiguration().Set("VaporNotes:NoteDuration", t.ToString("c"));
}
