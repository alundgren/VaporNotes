using Newtonsoft.Json;
using VaporNotes.Api.Domain;

namespace VaporNotes.Api;

public class DropboxTokenRefresher(string refreshToken, string appKey, string appSecret, IVaporNotesClock clock)
{
    public async Task<(string AccessToken, long ExpiresAtEpoch, string RefreshToken)> RefreshAccessToken()
    {
        using var client = new HttpClient();

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
    private record DropboxRefreshedToken(string access_token, int expires_in, string refresh_token);
}