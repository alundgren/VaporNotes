# Vapor notes

## Run locally

> ng serve

## Run exposed to local network

Create a file environments/environment.localNetwork.ts that is the exact same as development expect the apiBaseUrl points to
whatever you setup the api as. (See the api project README)

> ng serve --open --host <your local ip> --disable-host-check --configuration localNetwork

If on windows you can use ng-serve-localNetwork.ps1 which does the same but parses the ip from the environments file automatically:

> .\ng-serve-localNetwork.ps1

# Environment settings
Found in the /environments.

- apiBaseUrl: Location of the backend api
- isDebugLogEnabled: console logging of various things
- isProduction: true/false. When false some extra test functions are exposed.
- googleClientId: Client id for google login

# Google login

Handle client secret and redirect urls here:
https://console.cloud.google.com/

Guide to google login:
https://developers.google.com/identity/gsi/web/guides/overview

Promising guide:
https://dev.to/christianzink/how-to-sign-in-with-google-in-angular-and-use-jwt-based-net-core-api-authentication-rsa-1anl

# Docker
Build
> docker build -t irudd/vapornotes-ui-image:latest  .

Run
> docker run -d -p 8083:80 --rm --name vapornotes-ui irudd/vapornotes-ui-image:latest

Where 8083 is whatever the local port is.

Attach to running
> docker attach vapornotes-ui

Remove the container
> docker rm -f vapornotes-ui

Get a container shell
> docker exec - vapornotes-ui bash
