using System.Collections.Concurrent;
using VaporNotes.Api.Domain;

namespace VaporNotes.UnitTests;

internal class FakeDropboxService : IDropboxService
{
    private ConcurrentDictionary<string, byte[]> filesByPath = new ConcurrentDictionary<string, byte[]>();

    public Task<DropboxRefreshableAccessToken> CompleteAuthorizationAsync(string code)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFilesAsync(List<DropboxFileReference> ids)
    {
        ids.ForEach(id => filesByPath.Remove(id.Path, out var _));
        return Task.CompletedTask;
    }

    public Uri GetBeginAuthorizationUri()
    {
        throw new NotImplementedException();
    }

    public Task<Stream?> LoadFileAsync(DropboxFileReference file)
    {
        if (filesByPath.TryGetValue(file.Path, out var value))
            return Task.FromResult((Stream?)new MemoryStream(value));
        else
            return Task.FromResult((Stream?)null);
    }

    public Task<DropboxRefreshableAccessToken> RefreshAuthorizationAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task SaveFileAsync(Stream content, DropboxFileReference file)
    {
        var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        filesByPath[file.Path] = ms.ToArray();
    }
}
