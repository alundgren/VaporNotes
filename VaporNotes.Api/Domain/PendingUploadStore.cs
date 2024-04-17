using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace VaporNotes.Api.Domain;

public class PendingUploadStore
{
    private ConcurrentDictionary<string, UploadFileMetadata> pendingUploads = new ConcurrentDictionary<string, UploadFileMetadata>();

    public string CreateUploadKey(UploadFileMetadata file)
    {
        var key = Guid.NewGuid().ToString();
        pendingUploads[key] = file;
        return key;
    }

    public UploadFileMetadata? ConsumeUploadUploadKey(string key) =>
        pendingUploads.TryRemove(key, out var file) ? file : null;
}

public record UploadFileMetadata([Required]string FileName);
