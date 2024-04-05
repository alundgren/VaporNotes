using Dropbox.Api;
using System.Text;
using VaporNotes.Api;
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
app.MapPost("/api/notes/list", async (ListNotesRequest request) =>
{
    var client = new DropboxClient(request.AccessToken);
    var items = await client.Files.ListFolderAsync(new Dropbox.Api.Files.ListFolderArg(""));
    return items.Entries.Where(x => x.IsFile).Select(x => x.AsFile).Select(x => new
    {
        x.PreviewUrl,
        x.PathDisplay,
        x.PathLower,
        x.Id
    }).ToList();
});
app.MapPost("/api/notes/add-text", async (AddTextNoteRequest request) =>
{
    var client = new DropboxClient(request.AccessToken);
    var result = await client.Files.UploadAsync(new Dropbox.Api.Files.UploadArg("/test.txt"), new MemoryStream(Encoding.UTF8.GetBytes(request.Text)));
    return result.Id;
});
app.MapGet("/api/heartbeat", () =>
{
    return "Ok";
});

/*
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/
app.Run();