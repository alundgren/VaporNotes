# Build and publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish "VaporNotes.Api.csproj" -c Release -o /app

# Host
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./VaporNotes.Api"]
