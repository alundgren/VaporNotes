using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using VaporNotes.Api;
using VaporNotes.Api.Database;
using VaporNotes.Api.Domain;
using VaporNotes.Api.GoogleAuthentication;
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
            policy.WithExposedHeaders("Content-Disposition"); //File download does not work properly otherwise
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer();
builder.Services.AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddAuthorization(options =>
{
    // Define a default authorization policy that requires authentication
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
    //TODO: Enable after figuring out how to allow anon for the auth endpoint
    //options.FallbackPolicy = options.DefaultPolicy;
});
/*
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = StaticKeyAuthenticateScheme;
    //options.AddScheme("StaticKeyAuthenticateScheme", x => x.);
});

*/
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddTransient<VaporNotesBearerToken>();
builder.Services.AddSingleton<IVaporNotesClock, VaporNotesClock>();
builder.Services.AddTransient<VaporNotesService>();
builder.Services.AddSingleton<PendingUploadStore>();
builder.Services.AddSingleton<IDatabaseConnectionFactory, InMemoryDatabaseConnectionFactory>();

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
/*
app.UseAuthenticationLogged();
app.UseAuthorizationLogged();
*/
var clock = new VaporNotesClock();
app.MapPost("/api/notes/list", async (VaporNotesService service, ListNotesRequest request, HttpContext context) =>
{
     return await service.GetNotesAsync();
});
app.MapPost("/api/notes/add-text", async (VaporNotesService service, AddTextNoteRequest request) => await service.AddNoteAsync(request.Text));
app.MapPost("/api/heartbeat", () => "Ok").RequireAuthorization();
app.MapGet("/api/test-delay", async () =>
{
    await Task.Delay(5000);
    return "Ok";
});

//TODO: Would be better if we could upload directly to dropbox.
app.MapPost("/api/upload/begin", async (VaporNotesService service, [Required]UploadFileMetadata file) => new
{
    UploadKey = await service.CreateSingleUseUploadKeyAsync(file)
});
app.MapPost("/api/upload/{uploadKey}", async ([Required][FromForm] IFormFile file, [Required][FromRoute]string uploadKey, VaporNotesService service) =>
{
    using var stream = file.OpenReadStream();
    return await service.CompleteUploadAsync(uploadKey, stream);
})
.DisableAntiforgery();

app.MapGet("/api/download/attached-file/{noteId}", async ([Required][FromRoute] string noteId, VaporNotesService service) =>
{
    var result = await service.DownloadAttachedFile(noteId);
    if (!result.HasValue)
        return Results.NotFound();

    //TODO: contentType
    return Results.File(result.Value.Data, fileDownloadName: result.Value.Filename);
});

app.MapPost("/api/id-test", async ([Required] AuthenticateRequest request, HttpContext context) =>
{
    var generator = new JwtGenerator(JwtGenerator.PrivateKey);

    GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();

    // Change this to your google client ID
    settings.Audience = new List<string>() { "247237318435-g18gfog0e05vf6c7r8adeo1k9imvqfa4.apps.googleusercontent.com" };

    GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
    return new { AuthToken = generator.CreateUserAuthToken(payload.Email) };
})
.AllowAnonymous();

app.Run();