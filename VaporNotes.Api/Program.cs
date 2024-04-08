using Dropbox.Api;
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<VaporNotesBearerToken>();
builder.Services.AddSingleton<IVaporNotesClock, VaporNotesClock>();
builder.Services.AddTransient<IDropboxService, DropboxService>();
builder.Services.AddTransient(x => 
    new VaporNotesService(x.GetRequiredService<IDropboxService>(), 
    x.GetRequiredService<IVaporNotesClock>(), 
    TimeSpan.FromMinutes(1)));

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
app.MapPost("/api/begin-authorize", (IDropboxService dropbox) => dropbox.GetBeginAuthorizationUri());
app.MapPost("/api/complete-authorize", async (IDropboxService dropbox, CompleteAuthorizeRequest request) => await dropbox.CompleteAuthorizationAsync(request.Code));
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

app.MapPost("/api/notes/list", async (VaporNotesBearerToken token, ListNotesRequest request) =>
{
    var s = CreateService(new DropboxAccessToken(token.RequiredAccessToken));
    return await s.GetNotesAsync();
});
app.MapPost("/api/notes/add-text", async (VaporNotesBearerToken token, AddTextNoteRequest request) =>
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