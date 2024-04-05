using Dropbox.Api;
using Dropbox.Api.Files;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Dropbox;

public class DropboxService(IConfiguration configuration, DropboxAccessToken accessToken) : IDropboxService
{
    public string AppKey => configuration.GetRequiredSettingValue("VaporNotes:DropboxAppKey");
    public string AppSecret => configuration.GetRequiredSettingValue("VaporNotes:DropboxAppSecret");
    
    public async Task DeleteFilesAsync(List<DropboxFileReference> ids)
    {
        var client = new DropboxClient(accessToken.Token);
        await client.Files.DeleteBatchAsync(new DeleteBatchArg(ids.Select(x => new DeleteArg(GetApiPath(x))).ToList()));        
    }

    public async Task<Stream?> LoadFileAsync(DropboxFileReference file)
    {
        var client = new DropboxClient(accessToken.Token);
        try
        {
            var result = await client.Files.DownloadAsync(new DownloadArg(GetApiPath(file)));
            return await result.GetContentAsStreamAsync();
        } 
        catch(ApiException<DownloadError> ex)
        {
            if (ex.ErrorResponse.IsPath && ex.ErrorResponse.AsPath.Value.IsNotFound)
                return null;
            else
                throw;
        }
    }

    public async Task SaveFileAsync(Stream content, DropboxFileReference file)
    {
        
        var client = new DropboxClient(accessToken.Token);
        await client.Files.UploadAsync(new UploadArg(GetApiPath(file), mode: WriteMode.Overwrite.Instance), content);
    }

    private string GetApiPath(DropboxFileReference file) => $"{(file.Path.StartsWith("/") ? "" : "/")}{file.Path}";
}

public record DropboxAccessToken(string Token);
