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

