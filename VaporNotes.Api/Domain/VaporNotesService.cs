using System.Text;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Domain;

public class VaporNotesService(IDropboxService dropbox, IVaporNotesClock clock, IConfiguration configuration)
{
    //TODO: Make notes local to cache per session

    public async Task<List<Note>> GetNotesAsync()
    {
        var notes = await LoadNotesAsync();
        return await VaporizeNotesAsync(notes);
    }

    public async Task<List<Note>> AddNoteAsync(string text)
    {
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        notes.Add(new Note(Guid.NewGuid().ToString(), text, now, now.Add(NoteDuration), null, null));

        return await VaporizeNotesAsync(notes, alwaysSave: true);
    }

    /// <summary>
    /// 1. Call BeginUploadAsync to get a SingleUseUploadUrl
    /// 2. Upload from the app directly to SingleUseUploadUrl
    /// 3. Call CompleteUploadAsync
    /// </summary>
    public async Task<(List<Note> Notes, string UploadNoteId, Uri SingleUseUploadUrl)> BeginUploadAsync(string fileName)
    {
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        var newNoteId = Guid.NewGuid().ToString();
        notes.Add(new Note(newNoteId, fileName, now, now.Add(NoteDuration), new DropboxFileReference($"attached_{newNoteId}.dat"), true));

        await VaporizeNotesAsync(notes, alwaysSave: true);

        throw new NotImplementedException();
    }

    public async Task<List<Note>> CompleteUploadAsync(string uploadNoteId)
    {
        var notes = await LoadNotesAsync();
        var uploadNote = notes.Single(x => x.Id == uploadNoteId);
        //TODO: Check that the file is now on dropbox
        //TODO: Swap from records to classes so we can modify here
        uploadNote = uploadNote with { IsPendingUpload = false };
        notes = notes.Select(x => x.Id == uploadNote.Id ? uploadNote : x).ToList();
        
        return await VaporizeNotesAsync(notes, alwaysSave: true);
    }

    private async Task<List<Note>> VaporizeNotesAsync(List<Note> notes, bool alwaysSave = false)
    {
        var now = clock.UtcNow;
        bool IsExpired(Note note) => note.ExpirationDate < now;
        List<Note> notesToVaporize = notes.Where(IsExpired).ToList();
        if (notesToVaporize.Count == 0)
        {
            if(alwaysSave)
            {
                await SaveNotesAsync(notes);
            }
            return notes;
        }
            

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

    private TimeSpan NoteDuration => TimeSpan.ParseExact(configuration.GetRequiredSettingValue("VaporNotes:NoteDuration"), "c", null);

    private static DropboxFileReference NotesFileReference = new DropboxFileReference("notes_v1.json");
}

public interface IDropboxService
{
    Task SaveFileAsync(Stream content, DropboxFileReference file);
    Task<Stream?> LoadFileAsync(DropboxFileReference file);
    Task<bool> ExistsFileAsync(DropboxFileReference file);
    Task DeleteFilesAsync(List<DropboxFileReference> ids);
    Uri GetBeginAuthorizationUri();
    Task<DropboxRefreshableAccessToken> CompleteAuthorizationAsync(string code);
    Task<DropboxRefreshableAccessToken> RefreshAuthorizationAsync(string refreshToken);
}

public interface IVaporNotesClock
{
    DateTimeOffset UtcNow { get; }
}

public record Note(string Id, string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, DropboxFileReference? AttachedDropboxFile, bool? IsPendingUpload);
public record DropboxFileReference(string Path);

public record DropboxRefreshableAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);