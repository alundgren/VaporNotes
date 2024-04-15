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