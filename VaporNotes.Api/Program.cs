using Dropbox.Api;
using VaporNotes.Api;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Dropbox;
using VaporNotes.Api.Support;
using static Dropbox.Api.TeamLog.EventCategory;

const string ApiCorsPolicyName = "UiApiCallsCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    var uiBaseUrl = new Uri(builder.Configuration.GetRequiredSettingValue("VaporNotes:UiBaseUrl"));
    options.AddPolicy(name: ApiCorsPolicyName,
        policy =>
        {
            policy.WithOrigins(uiBaseUrl.ToString(), uiBaseUrl.ToString().TrimEnd('/'));
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
        });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerToken>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(ApiCorsPolicyName);

var appKey = builder.Configuration.GetRequiredSettingValue("VaporNotes:DropboxAppKey");
var appSecret = builder.Configuration.GetRequiredSettingValue("VaporNotes:DropboxAppSecret");
var clock = new VaporNotesClock();
app.MapPost("/api/begin-authorize", () =>
{
    var loginUrl = Dropbox.Api.DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, appKey, redirectUri: default(string), tokenAccessType: TokenAccessType.Offline);
    return new
    {
        LoginUrl = loginUrl
    }; 
});
app.MapPost("/api/complete-authorize", async (CompleteAuthorizeRequest request) =>
{
    var result = await DropboxOAuth2Helper.ProcessCodeFlowAsync(request.Code, appKey, appSecret: appSecret);    
    return new
    {
        ExpiresAtEpoch = result.ExpiresAt.HasValue 
            ? new DateTimeOffset(result.ExpiresAt.Value).ToUnixTimeMilliseconds()
            : clock.UtcNow.AddHours(1).ToUnixTimeMilliseconds(),
        result.AccessToken,
        result.RefreshToken
    };
});
app.MapPost("/api/refresh-authorize", async (RefreshAuthorizeRequest request) =>
{
    var refresher = new DropboxTokenRefresher(request.RefreshToken, appKey, appSecret, new VaporNotesClock());
    var result = await refresher.RefreshAccessToken();
    return new
    {
        ExpiresAtEpoch = result.ExpiresAtEpoch,
        result.AccessToken,
        result.RefreshToken
    };
});

VaporNotesService CreateService(DropboxAccessToken accessToken)
{
    var d = new DropboxService(builder.Configuration, accessToken);
    return new VaporNotesService(d, new VaporNotesClock(), TimeSpan.FromMinutes(1));
}

app.MapPost("/api/notes/list", async (BearerToken token, ListNotesRequest request) =>
{
    var s = CreateService(new DropboxAccessToken(token.RequiredAccessToken));
    return await s.GetNotesAsync();
});
app.MapPost("/api/notes/add-text", async (BearerToken token, AddTextNoteRequest request) =>
{
    var s = CreateService(new DropboxAccessToken(token.RequiredAccessToken));
    return await s.AddNoteAsync("Test: " + DateTime.UtcNow.ToString("o"));
});
app.MapGet("/api/heartbeat", () =>
{
    return "Ok";
});

/*
.WithName("GetWeatherForecast")
.WithOpenApi();
*/
app.Run();