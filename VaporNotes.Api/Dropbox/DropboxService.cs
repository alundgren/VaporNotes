using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Dropbox;

public class DropboxService(IConfiguration configuration, IVaporNotesClock clock, VaporNotesBearerToken accessToken) : IDropboxService
{
    public string AppKey => configuration.GetRequiredSettingValue("VaporNotes:DropboxAppKey");
    public string AppSecret => configuration.GetRequiredSettingValue("VaporNotes:DropboxAppSecret");

    public async Task DeleteFilesAsync(List<DropboxFileReference> ids)
    {
        var client = new DropboxClient(accessToken.RequiredAccessToken);
        await client.Files.DeleteBatchAsync(new DeleteBatchArg(ids.Select(x => new DeleteArg(GetApiPath(x))).ToList()));        
    }

    public Uri GetBeginAuthorizationUri() =>
        DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, AppKey, redirectUri: default(string), tokenAccessType: TokenAccessType.Offline);

    public async Task<DropboxRefreshableAccessToken> CompleteAuthorizationAsync(string code)
    {
        var result = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, AppKey, appSecret: AppSecret);
        return new DropboxRefreshableAccessToken(result.AccessToken, result.RefreshToken, result.ExpiresAt.HasValue
                ? new DateTimeOffset(result.ExpiresAt.Value)
                : clock.UtcNow.AddHours(1));
    }

    public async Task<Stream?> LoadFileAsync(DropboxFileReference file)
    {
        var client = new DropboxClient(accessToken.RequiredAccessToken);
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
        
        var client = new DropboxClient(accessToken.RequiredAccessToken);
        await client.Files.UploadAsync(new UploadArg(GetApiPath(file), mode: WriteMode.Overwrite.Instance), content);
    }

    private string GetApiPath(DropboxFileReference file) => $"{(file.Path.StartsWith("/") ? "" : "/")}{file.Path}";

    public Task<DropboxRefreshableAccessToken> RefreshAuthorizationAsync(string refreshToken)
    {
        using var client = new HttpClient();

        todo httpclient factory

        var url = "https://api.dropboxapi.com/oauth2/token";
        var data = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", appKey },
            { "client_secret", appSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(data)
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var tokenResult = JsonConvert.DeserializeObject<DropboxRefreshedToken>(jsonContent);
        if (tokenResult?.access_token == null)
            throw new Exception("Missing token in response");
        return (tokenResult.access_token, clock.UtcNow.AddMinutes(tokenResult.expires_in).ToUnixTimeMilliseconds(), tokenResult.refresh_token ?? refreshToken);
    }
}


public class DropboxAccessToken(string? token)
{
    public string RequireToken()
    {
        if (token == null)
            throw new Exception("Missing access token");
        return token;
    }
}
