using Dapper;
using Microsoft.Data.Sqlite;
using System.Text;
using VaporNotes.Api.Database;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Domain;

public class VaporNotesService(IVaporNotesClock clock, IConfiguration configuration, PendingUploadStore pendingUploadStore, IDatabaseConnectionFactory connectionFactory)
{
    //TODO: Move notes to it's own table with one note per row instead after getting rid of all dropbox traces
    private const string NotesFileId = "d14f8b3c-775c-4ab0-9758-03c16c32a8e3";

    private SqliteConnection Connection = connectionFactory.CreateConnection();

    private async Task<(long FileLength, string ContentType, string FileName, Stream FileData)?> LoadFileAsync(string fileId)
    {
        //TODO: return all metadata
        //(Id TEXT NOT NULL PRIMARY KEY, ContentType TEXT NOT NULL, FileName TEXT NOT NULL, FileData BLOB
        var row = await Connection.QueryFirstOrDefaultAsync<FileMetadata?>("select rowid as RowId, length(FileData) as DataLength, ContentType, FileName from File where Id = @id", new { id = fileId });
        if (row == null)
            return null;
        var blob = new SqliteBlob(Connection, "File", "FileData", row.RowId, readOnly: true); //TODO: Who disposes this?
        return (row.DataLength, row.ContentType, row.FileName, blob);
    }

    private async Task SaveFileAsync(string fileId, Stream fileData, string contentType, string fileName)
    {
        //TODO: Update instead of delete first if exists.
        await Connection.ExecuteAsync("delete from File where Id = @id", new { id = fileId });
        var rowId = await Connection.ExecuteScalarAsync<long>(@"INSERT INTO File(Id, ContentType, FileName, FileData) VALUES (@id, @contentType, @fileName, zeroblob(@length));SELECT last_insert_rowid();", 
            new 
            { 
                id = fileId, 
                length = fileData.Length,
                contentType = contentType,
                fileName = fileName
            });
        using var blob = new SqliteBlob(Connection, "File", "FileData", rowId, readOnly: false);
        await fileData.CopyToAsync(blob);
    }

    private async Task DeleteFilesAsync(List<string> fileIds)
    {
        await Connection.ExecuteAsync("delete from File where Id in (@ids)", new { ids = fileIds });
    }

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

        //TODO: Include content type in upload (and split it so we insert on begin and just stream in the data here)
        await SaveFileAsync(noteId, file, "application/octet-stream", fileMetadata.FileName);
        var notes = await LoadNotesAsync();
        var now = clock.UtcNow;
        notes.Add(new Note(noteId, fileMetadata.FileName, now, now.Add(NoteDuration), noteId));
        return await VaporizeNotesAsync(notes, alwaysSave: true);        
    }

    public async Task<(Stream Data, string Filename)?> DownloadAttachedFile(string noteId)
    {
        var notes = await LoadNotesAsync();
        var note = notes.SingleOrDefault(x => x.Id == noteId);

        var fileId = note?.AttachedFileId;
        if (fileId == null) 
            return null;

        var file = await LoadFileAsync(fileId);
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

        List<string> attachedFileIdsToDelete = notesToVaporize
            .Where(x => x.AttachedFileId != null)
            .Select(x => x.AttachedFileId!)
            .ToList();

        if (attachedFileIdsToDelete.Count > 0)
            await DeleteFilesAsync(attachedFileIdsToDelete);
        
        notes = notes.Where(x => !IsExpired(x)).ToList();
        await SaveNotesAsync(notes);
        return notes;
    }

    private async Task SaveNotesAsync(List<Note> notes)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(notes)));
        
        await SaveFileAsync(NotesFileId, stream, "application/json", "notesv1.json" );
    }

    private async Task<List<Note>> LoadNotesAsync()
    {
        var file = await LoadFileAsync(NotesFileId);
        if (file == null)
            return new List<Note>();

        using StreamReader s = new(file.Value.FileData);
        string fileContent = await s.ReadToEndAsync();
        var parsedNotes = System.Text.Json.JsonSerializer.Deserialize<List<Note>>(fileContent);
        if (parsedNotes == null)
            throw new Exception("Failed to parse notes");
        return parsedNotes;
    }

    private TimeSpan NoteDuration => TimeSpan.ParseExact(configuration.GetRequiredSettingValue("VaporNotes:NoteDuration"), "c", null);

    private record FileMetadata(long RowId, long DataLength, string ContentType, string FileName);
}

public interface IVaporNotesClock
{
    DateTimeOffset UtcNow { get; }
}

public record Note(string Id, string Text, DateTimeOffset CreationDate, DateTimeOffset ExpirationDate, string? AttachedFileId);
