## Settings summary
- VaporNotes:UiBaseUrl: Base url of the web app
- VaporNotes:GoogleClientId: Client id for google authentication (Note: exposed publicly in the web app so this is not a secret)
- VaporNotes:AccessPublicKey: Public key for the access token jwt
- VaporNotes:AccessPrivateKey: Private key for the acces token jwt

## Authentication
- The uses logs in the their google account in the ui and passes the id token to api/authenticate.
- This issues them an access token that can be used with all the other apis.

Besides the client id for google auth that must match what is set in a public/private keypair is used to sign the access token jwt. 
This will be automatically generated if VaporNotes:JwtSigningKey is missing but lost when the api recycles if used this way.

## Local development secrets
Add app key/client id and app secret/client secret:
> dotnet user-secrets set "VaporNotes:JwtSigningKey" "<...>"

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

Or run interactive
> docker run -it -p 8084:8080 --rm --env-file prod.env --name vapornotes-api irudd/vapornotes-api-image:latest

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