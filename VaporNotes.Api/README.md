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
- Make sure to set UiBaseUrl in appsettings LocalNetwork and applicationUrl in launchSettings to your local ip.
- The angular ui alsow needs to do the same
- 
Run in launch proifle localNetwork:

> dotnet run -lp localNetwork

- Setup in local.appsettings.json. (do NOT remove this from .gitignore)
- It should have VaporNotes:UiBaseUrl with the same url as is exposed in launchSettings
- NOTE: It would be way better if this was an environment like LocalNetwork with it's own user secrets and appsettings.LocalNetwork.json but uses secrets seem to not work at all even when 
        using the -c param and there seems to be no way of adding to the list of names that the framework considers IsDevelopment so this is the comprompise for now.
