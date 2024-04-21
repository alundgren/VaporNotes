using System.Text;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Domain;

public class VaporNotesService(IDropboxService dropbox, IVaporNotesClock clock, IConfiguration configuration, PendingUploadStore pendingUploadStore)
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
        notes.Add(new Note(Guid.NewGuid().ToString(), text, now, now.Add(NoteDuration), null));

        return await VaporizeNotesAsync(notes, alwaysSave: true);
    }

    /// <summary>
    /// 1. Call BeginUploadAsync to get a SingleUseUploadUrl
    /// 2. Upload from the app directly to SingleUseUploadUrl
    /// 3. Call CompleteUploadAsync
    /// </summary>
    public Task<string> CreateSingleUseUploadKeyAsync(UploadFileMetadata file) =>
        Task.FromResult(pendingUploadStore.CreateUploadKey(file));    

    public async Task<List<Note>> CompleteUploadAsync(string uploadKey, Stream file)
    {        
        var fileMetadata = pendingUploadStore.ConsumeUploadUploadKey(uploadKey);
        if (fileMetadata == null) //TODO: Custom exception -> user friendly error
            throw new Exception("Missing upload key");
                   
        var noteId = Guid.NewGuid().ToString();
        var dropboxReference = new DropboxFileReference($"attached-file/{noteId}.dat");
        await dropbox.SaveFileAsync(file, dropboxReference);
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        notes.Add(new Note(noteId, fileMetadata.FileName, now, now.Add(NoteDuration), dropboxReference));
        return await VaporizeNotesAsync(notes, alwaysSave: true);        
    }

    public async Task<(Stream Data, string Filename)?> DownloadAttachedFile(string noteId)
    {
        var notes = await LoadNotesAsync();
        var note = notes.SingleOrDefault(x => x.Id == noteId);
        var file = note?.AttachedDropboxFile;
        if (note == null || file == null)
            return null;
        var fileStream = await dropbox.LoadFileAsync(file);
        if(fileStream == null)
            return null;
        return (fileStream, note.Text);
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
    Task DeleteFilesAsync(List<DropboxFileReference> ids);
    Uri GetBeginAuthorizationUri();
    Task<DropboxRefreshableAccessToken> CompleteAuthorizationAsync(string code);
    Task<DropboxRefreshableAccessToken> RefreshAuthorizationAsync(string refreshToken);
}

public interface IVaporNotesClock
{
    DateTimeOffset UtcNow { get; }
}

public record Note(string Id, string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, DropboxFileReference? AttachedDropboxFile);
public record DropboxFileReference(string Path);
public record DropboxRefreshableAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
