using Dropbox.Api;
using System.Text;
using VaporNotes.Api;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Dropbox;
using VaporNotes.Api.Support;

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

app.MapPost("/api/begin-authorize", () =>
{
    var uiBaseUrl = new Uri(builder.Configuration.GetRequiredSettingValue("VaporNotes:UiBaseUrl"));
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
        result.ExpiresAt,
        result.AccessToken,
        result.RefreshToken
    };
});

VaporNotesService CreateService(DropboxAccessToken accessToken)
{
    var d = new DropboxService(builder.Configuration, accessToken);
    return new VaporNotesService(d, new VaporNotesClock(), TimeSpan.FromMinutes(1));
}

app.MapPost("/api/notes/list", async (ListNotesRequest request) =>
{
    var s = CreateService(new DropboxAccessToken(request.AccessToken));
    return await s.GetNotesAsync();
});
app.MapPost("/api/notes/add-text", async (AddTextNoteRequest request) =>
{
    var s = CreateService(new DropboxAccessToken(request.AccessToken));
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