# Vapor notes
Run locally:

> ng serve

Run exposed to local network:

> ng serve --open --host <your local ip> --disable-host-check --configuration localNetwork

This will also require setting http://<your local ip>/<:port> in the api setting VaporNotes:UiBaseUrl or CORS will prevent calls.

# Environment settings
Found in the /environments.

- apiBaseUrl: Location of the backend api
- isDebugLogEnabled: console logging of various things
- isProduction: true/false. When false some extra test functions are exposed.