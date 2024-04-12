using VaporNotes.Api;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Dropbox;
using VaporNotes.Api.Support;

const string ApiCorsPolicyName = "UiApiCallsCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); //https://aka.ms/aspnetcore/swashbuckle
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
builder.Services.AddHttpClient();
builder.Services.AddTransient<VaporNotesBearerToken>();
builder.Services.AddSingleton<IVaporNotesClock, VaporNotesClock>();
builder.Services.AddTransient<IDropboxService, DropboxService>();
builder.Services.AddTransient<VaporNotesService>();

var app = builder.Build();

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
app.MapPost("/api/complete-authorize", async (IDropboxService dropbox, CompleteAuthorizeRequest request) =>
{
    var result = await dropbox.CompleteAuthorizationAsync(request.Code);
    return new
    {
        ExpiresAtEpoch = result.ExpiresAt.ToUnixTimeMilliseconds(),
        result.AccessToken,
        result.RefreshToken
    };
});
app.MapPost("/api/refresh-authorize", async (IDropboxService dropbox, RefreshAuthorizeRequest request) =>
{
    var result = await dropbox.RefreshAuthorizationAsync(request.RefreshToken);
    return new
    {
        ExpiresAtEpoch = result.ExpiresAt.ToUnixTimeMilliseconds(),
        result.AccessToken,
        result.RefreshToken
    };
});

app.MapPost("/api/notes/list", async (VaporNotesService service, ListNotesRequest request) => await service.GetNotesAsync());
app.MapPost("/api/notes/add-text", async (VaporNotesService service, AddTextNoteRequest request) => await service.AddNoteAsync(request.Text));
app.MapGet("/api/heartbeat", () => "Ok");
app.MapGet("/api/test-delay", async () =>
{
    await Task.Delay(5000);
    return "Ok";
});

/*
.WithName("GetWeatherForecast")
.WithOpenApi();
*/
app.Run();