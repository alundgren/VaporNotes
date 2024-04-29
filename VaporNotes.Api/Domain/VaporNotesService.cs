using Dapper;
using Microsoft.AspNetCore.Connections;
using Microsoft.Data.Sqlite;
using System.Net.Mime;
using System.Text;
using VaporNotes.Api.Database;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Domain;

public class VaporNotesService(IDropboxService dropbox, IVaporNotesClock clock, IConfiguration configuration, PendingUploadStore pendingUploadStore, IDatabaseConnectionFactory connectionFactory)
{
    //TODO: Move notes to it's own table with one note per row instead after getting rid of all dropbox traces
    private const string NotesFileId = "d14f8b3c-775c-4ab0-9758-03c16c32a8e3";

    private async Task<(long FileLength, string ContentType, string FileName, Stream FileData)?> LoadFileAsync(string fileId, SqliteConnection connection)
    {
        //TODO: return all metadata
        //(Id TEXT NOT NULL PRIMARY KEY, ContentType TEXT NOT NULL, FileName TEXT NOT NULL, FileData BLOB
        var row = await connection.QueryFirstOrDefaultAsync<FileMetadata?>("select rowid as RowId, length(FileData) as DataLength, ContentType, FileName from File where Id = @id", new { id = fileId });
        if (row == null)
            return null;
        var blob = new SqliteBlob(connection, "File", "FileData", row.RowId, readOnly: true); //TODO: Who disposes this?
        return (row.DataLength, row.ContentType, row.FileName, blob);
    }

    private async Task SaveFileAsync(string fileId, long fileDataLength, Stream fileData, SqliteConnection connection, string contentType, string fileName)
    {
        //TODO: Update instead of delete first if exists.
        await connection.ExecuteAsync("delete from File where Id = @id", new { id = fileId });
        var rowId = await connection.ExecuteScalarAsync<long>(@"INSERT INTO File(Id, ContentType, FileName, FileData) VALUES (@id, @contentType, @fileName, zeroblob(@length));SELECT last_insert_rowid();", 
            new 
            { 
                id = fileId, 
                length = fileDataLength,
                contentType = contentType,
                fileName = fileName
            });
        using var blob = new SqliteBlob(connection, "File", "FileData", rowId, readOnly: false);
        await fileData.CopyToAsync(blob);
    }

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
        using var conn = connectionFactory.CreateConnection();

        //TODO: Include content type in upload (and split it so we insert on begin and just stream in the data here)
        await SaveFileAsync(noteId, file.Length, file, conn, "application/octet-stream", fileMetadata.FileName);
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        notes.Add(new Note(noteId, fileMetadata.FileName, now, now.Add(NoteDuration), noteId));
        return await VaporizeNotesAsync(notes, alwaysSave: true);        
    }

    public async Task<(Stream Data, string Filename)?> DownloadAttachedFile(string noteId)
    {
        using var conn = connectionFactory.CreateConnection();

        var notes = await LoadNotesAsync();
        var note = notes.SingleOrDefault(x => x.Id == noteId);

        var fileId = note?.AttachedFileId;
        if (fileId == null) 
            return null;

        var file = await LoadFileAsync(fileId, conn);
        if (file == null)
            return null;

        return (file.Value.FileData, file.Value.FileName);
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
    private record FileMetadata(long RowId, long DataLength, string ContentType, string FileName);
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

public record Note(string Id, string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, string? AttachedFileId);
public record DropboxFileReference(string Path);
public record DropboxRefreshableAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
