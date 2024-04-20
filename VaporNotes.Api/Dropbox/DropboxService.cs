using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Support;
using static Dropbox.Api.Sharing.ListFileMembersIndividualResult;

namespace VaporNotes.Api.Dropbox;

public class DropboxService(IConfiguration configuration, IVaporNotesClock clock, VaporNotesBearerToken accessToken, IHttpClientFactory httpClientFactory) : IDropboxService
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

    public async Task<bool> ExistsFileAsync(DropboxFileReference file)
    {
        var client = new DropboxClient(accessToken.RequiredAccessToken);
        try
        {
            //TODO: Use list and make this a batch api instead
            var metaData = await client.Files.GetMetadataAsync(new GetMetadataArg(GetApiPath(file)));
            
            var result = await client.Files.DownloadAsync(new DownloadArg(GetApiPath(file)));

            return true;
        }
        catch (ApiException<DownloadError> ex)
        {
            if (ex.ErrorResponse.IsPath && ex.ErrorResponse.AsPath.Value.IsNotFound)
                return false;
            else
                throw;
        }
    }

    public async Task SaveFileAsync(Stream content, DropboxFileReference file)
    {        
        var client = new DropboxClient(accessToken.RequiredAccessToken);
        await client.Files.UploadAsync(new UploadArg(GetApiPath(file), mode: WriteMode.Overwrite.Instance), content);
    }

    public async Task<(Uri Url, DateTimeOffset ExpirationDate)> CreatePublicDownloadLink(DropboxFileReference file, TimeSpan duration)
    {
        var client = new DropboxClient(accessToken.RequiredAccessToken);
        var expires = clock.UtcNow.Add(duration);
        try
        {
            var result = await client.Sharing.CreateSharedLinkWithSettingsAsync(new CreateSharedLinkWithSettingsArg(GetApiPath(file), new SharedLinkSettings(
                allowDownload: true,
                //expires: expires.DateTime,
                requirePassword: false,
                audience: LinkAudience.Public.Instance,
                access: RequestedLinkAccessLevel.Viewer.Instance,
                requestedVisibility: RequestedVisibility.Public.Instance,
                linkPassword: null)));
            return (new Uri(result.Url), expires);
        }
        catch(ApiException<CreateSharedLinkWithSettingsError> ex)
        {
            if (!ex.ErrorResponse.IsSharedLinkAlreadyExists)
                throw;
        }

        var existing = await client.Sharing.ListSharedLinksAsync(GetApiPath(file), directOnly: true);
        return (new Uri(existing.Links.Single().Url), expires);
    }

    private string GetApiPath(DropboxFileReference file) => $"{(file.Path.StartsWith("/") ? "" : "/")}{file.Path}";

    public async Task<DropboxRefreshableAccessToken> RefreshAuthorizationAsync(string refreshToken)
    {
        using var client = httpClientFactory.CreateClient();

        var url = "https://api.dropboxapi.com/oauth2/token";
        var data = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", AppKey },
            { "client_secret", AppSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(data)
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var tokenResult = await response.Content.ReadFromJsonAsync<DropboxRefreshResult>();
        if (tokenResult?.access_token == null)
            throw new Exception("Missing token in response");
        return new DropboxRefreshableAccessToken(tokenResult.access_token,
            tokenResult.refresh_token ?? refreshToken,
            clock.UtcNow.AddMinutes(tokenResult.expires_in ?? 60));
    }

    private record DropboxRefreshResult(string? access_token, int? expires_in, string? refresh_token);
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
