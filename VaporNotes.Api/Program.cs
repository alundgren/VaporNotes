using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using VaporNotes.Api;
using VaporNotes.Api.Domain;
using VaporNotes.Api.Dropbox;
using VaporNotes.Api.Support;

const string ApiCorsPolicyName = "UiApiCallsCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["VAPORNOTES_IS_LOCALNETWORK"] == "true")
{
    /*
     * This is used so we can keep launchSettings.json in version-control without keeping the local machines ip adress in applicationUrl.
     * We keep the ip in the localNetwork launch profile as see_readme_do_not_change and then override it using this file which
     * is kept out of version control. Note that this only works in Development.
     */
    builder.Configuration.AddJsonFile("localNetwork.appsettings.json", optional: true);
}


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
builder.Services.AddSingleton<PendingUploadStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();        
}
else
{
    app.UseHttpsRedirection();
}

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

//TODO: Would be better if we could upload directly to dropbox.
app.MapPost("/api/upload/begin", async (VaporNotesService service, [Required]UploadFileMetadata file) => await service.CreateSingleUseUploadKeyAsync(file));
app.MapPost("/api/upload/{uploadKey}", async ([Required][FromForm] IFormFile file, [Required][FromRoute]string uploadKey, VaporNotesService service) =>
{
    using var stream = file.OpenReadStream();
    return await service.CompleteUploadAsync(uploadKey, stream);
});


/*
.WithName("GetWeatherForecast")
.WithOpenApi();
*/
app.Run();