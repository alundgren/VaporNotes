## Dropbox api keys
Add app key/client id and app secret/client secret:
> dotnet user-secrets set "VaporNotes:DropboxAppKey" "<...>"
> dotnet user-secrets set "VaporNotes:DropboxAppSecret" "<...>"

If you get a wierd error doing this then first call:
> dotnet user-secrets init

## Duration of notes
Set the note duration with VaporNotes:NoteDuration.

The [format](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings#the-constant-c-format-specifier) is [d'.']hh':'mm':'ss['.'fffffff].

## Ui
Set up url for the ui to configure cors headers using the appsetting 'VaporNotes:UiBaseUrl'

## Testing on the local network
Create a local file localNetwork.appsettings.json. Example:

`
{
  "URLS": "http://192.168.0.184:3000",
  "VaporNotes:UiBaseUrl": "http://192.168.0.184:4200/"
}
`

- URLS: Override the static applicationUrl = see_readme_do_not_change in launchsettings.json (so it can be kept in version control).
- VaporNotes:UiBaseUrl: Synch with the ui to allow CORS access.

Then run the app as:

> dotnet run -lp localNetwork