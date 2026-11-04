# RaceDay — PROG6212 Part 3

RaceDay is an ASP.NET Core MVC frontend for the RaceDay REST API developed in Part 2.

## Solution structure

- `POE_Part2.csproj` — Part 2 REST API.
- `RaceDay.Web/` — Part 3 ASP.NET Core MVC frontend.
- `RaceDay.Web/Controllers/AccountController.cs` — API-backed registration, login and logout.
- `RaceDay.Web/Filters/SessionAuthorizeAttribute.cs` — session-based access and role checks.
- `RaceDay.Web/Controllers/OrganiserController.cs` — organiser dashboard entry point.
- `RaceDay.Web/Controllers/ParticipantController.cs` — participant dashboard entry point.

The MVC project communicates with the API over HTTP using the named `RaceDayApi` HttpClient. It does not access the database directly.

## Requirements

- .NET 8 SDK
- A running Part 2 RaceDay API
- SQL Server configured for the API

## Configure the API URL

Update `RaceDay.Web/appsettings.json` to match the HTTPS URL printed when the Part 2 API starts:

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7001/"
  }
}
```

Do not commit production credentials or secrets.

## Run locally

From the repository root:

```bash
dotnet restore POE_Part2.sln
dotnet run --project RaceDay.Web/RaceDay.Web.csproj
```

Start the Part 2 API in a separate terminal before testing registration and login.

## Run with Docker

Build the MVC image from the repository root:

```bash
docker build -f RaceDay.Web/Dockerfile -t raceday-web .
docker run --rm -p 8080:8080 -e Api__BaseUrl="http://host.docker.internal:7000/" raceday-web
```

The API URL is an example. Configure it to an address reachable from inside the container. For a multi-container deployment, use the API service name on a shared Docker network. Do not use localhost to refer to the host machine from inside the container.

## Current implementation status

The first Part 3 foundation includes session configuration, API-backed account registration/login, logout, role-protected dashboard routes and a responsive RaceDay interface. Event workflows, Azure Blob Storage, Docker runtime verification and full end-to-end testing remain to be completed.


## API authentication secret (required)

The API signs login tokens with a secret that must remain outside source control. From the repository root, initialise .NET user-secrets for the API project and set a unique local development key (at least 32 UTF-8 bytes):

```bash
dotnet user-secrets init --project POE_Part2.csproj
dotnet user-secrets set "Jwt:Key" "replace-this-with-a-unique-random-secret-of-at-least-32-characters" --project POE_Part2.csproj
```

For deployment, set the environment variable `Jwt__Key` using your hosting provider's secret manager. Do not commit the signing secret to appsettings.json or Git.

The API uses issuer `RaceDay.Api` and audience `RaceDay.Client` by default. Keep these values aligned with the MVC/API environment.


## Azure Blob Storage for event banners

The MVC application can upload JPG, PNG and WebP event banners to the Azure Blob Storage container configured as `event-banners`. Uploads are limited to 5 MB and use generated blob names rather than the original file name.

Set the storage connection string as an environment variable or in .NET user-secrets for the MVC project:

```bash
dotnet user-secrets init --project RaceDay.Web/RaceDay.Web.csproj
dotnet user-secrets set "Azure:StorageConnectionString" "<your Azure Storage connection string>" --project RaceDay.Web/RaceDay.Web.csproj
```

For deployment, configure `Azure__StorageConnectionString` in the hosting platform's secret settings. Do not commit the connection string.

The banner container is created with blob-level public read access because event banners are intended to be visible to participants. Ensure public blob access is enabled for the storage account. If your account policy prohibits anonymous blob access, change the service to generate time-limited read SAS URLs instead.
