## Settings summary
- VaporNotes:DropboxAppKey
- VaporNotes:DropboxAppSecret
- VaporNotes:UiBaseUrl


## Dropbox api keys
To edit the app go here:
https://www.dropbox.com/developers/apps

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

# Docker
Build
> docker build -t irudd/vapornotes-api-image:latest  .

Run
> docker run -d -p 8084:8080 --rm --env-file prod.env --name vapornotes-api irudd/vapornotes-api-image:latest

Where 8084 is whatever the local port is.
TODO: Add --env-file <...> here so we can keep prod credentials on the prod server. These are just <name>=<value> one per line.

Attach to running
> docker attach vapornotes-api

Remove the container
> docker rm -f vapornotes-api

Get a container shell
> docker exec -it vapornotes-api bash

Get a container shell when container insta exists
> docker run -it --entrypoint='' irudd/vapornotes-api-image:latest bash

Remove all containers
> docker rm -v -f $(docker ps -qa)