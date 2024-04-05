using System.Text;

namespace VaporNotes.Api.Domain;

public class VaporNotesService(IDropboxService dropbox, IVaporNotesClock clock, TimeSpan noteDuration)
{
    public async Task<List<Note>> GetNotesAsync()
    {
        var notes = await LoadNotesAsync();
        return await VaporizeNotesAsync(notes);
    }

    public async Task<List<Note>> AddNoteAsync(string text)
    {
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        notes.Add(new Note(text, now, now.Add(noteDuration), null));

        await SaveNotesAsync(notes);
        return await VaporizeNotesAsync(notes); //TODO: Could be optimized to prevent to saves on vaporize
    }

    private async Task<List<Note>> VaporizeNotesAsync(List<Note> notes)
    {
        var now = clock.UtcNow;
        bool IsExpired(Note note) => note.ExpirationDate < now;
        List<Note> notesToVaporize = notes.Where(IsExpired).ToList();
        if (notesToVaporize.Count == 0)
            return notes;

        List<DropboxFileReference> attachedFilesToDelete = notesToVaporize
            .Where(x => x.AttachedDropboxFile != null)
            .Select(x => x.AttachedDropboxFile!)
            .ToList();
        if (attachedFilesToDelete.Count > 0)
            await dropbox.DeleteFilesAsync(attachedFilesToDelete);
        
        notes = notes.Where(x => !IsExpired(x)).ToList();
        await SaveNotesAsync(notes);
        return notes;
    }

    private async Task SaveNotesAsync(List<Note> notes)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(notes)));
        await dropbox.SaveFileAsync(stream, NotesFileReference);
    }

    private async Task<List<Note>> LoadNotesAsync()
    {
        Stream? file = await dropbox.LoadFileAsync(NotesFileReference);
        if (file == null)
            return new List<Note>();

        using StreamReader s = new(file);
        string fileContent = await s.ReadToEndAsync();
        var parsedNotes = System.Text.Json.JsonSerializer.Deserialize<List<Note>>(fileContent);
        if (parsedNotes == null)
            throw new Exception("Failed to parse notes");
        return parsedNotes;
    }

    private static DropboxFileReference NotesFileReference = new DropboxFileReference("notes_v1.json");
}

public interface IDropboxService
{
    Task SaveFileAsync(Stream content, DropboxFileReference file);
    Task<Stream?> LoadFileAsync(DropboxFileReference file);
    Task DeleteFilesAsync(List<DropboxFileReference> ids);
}

public interface IVaporNotesClock
{
    DateTimeOffset UtcNow { get; }
}

public record Note(string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, DropboxFileReference? AttachedDropboxFile);
public record DropboxFileReference(string Path);