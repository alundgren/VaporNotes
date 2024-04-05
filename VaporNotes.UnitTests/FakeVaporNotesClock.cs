using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests;

internal class FakeVaporNotesClock(TimeSpan? timeSinceZero = null) : IVaporNotesClock
{
    private static DateTimeOffset zero = new DateTimeOffset(2024, 04, 05, 13, 32, 19, TimeSpan.Zero);
    public DateTimeOffset UtcNow { get; private set; } = zero.Add(timeSinceZero ?? TimeSpan.Zero);
    public void LetTimePass(TimeSpan duration) => UtcNow = UtcNow.Add(duration);
}
