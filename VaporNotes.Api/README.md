## Dropbox api keys
Add app key/client id and app secret/client secret:
> dotnet user-secrets set "VaporNotes:DropboxAppKey" "<...>"
> dotnet user-secrets set "VaporNotes:DropboxAppSecret" "<...>"

If you get a wierd error doing this then first call:
> dotnet user-secrets init

## Ui
Set up url for the ui to configure cors headers using the appsetting 'VaporNotes:UiBaseUrl'

