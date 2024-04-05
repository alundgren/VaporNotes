using VaporNotes.Api.Domain;

namespace VaporNotes.Api.Support
{
    public class VaporNotesClock : IVaporNotesClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
