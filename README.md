# POE PART 2 AND PART 3 — RaceDay

## Solution contents
- `POE_Part2`: ASP.NET Core Web API (JWT authentication and RaceDay endpoints).
- `RaceDay`: ASP.NET Core MVC website (Participant and Organiser interfaces).
- `POE_Part2.sln`: Visual Studio solution containing both projects.

## Requirements
- Visual Studio 2022/2026 with the ASP.NET and web development workload.
- .NET 8 SDK.
- Internet access on first build to restore NuGet packages.

## Open and run
1. Extract the complete ZIP to a local folder.
2. Open `POE_Part2/POE_Part2.sln` in Visual Studio.
3. In Solution Explorer, right-click the solution and choose **Set Startup Projects**.
4. Select **Multiple startup projects** and set both `POE_Part2` and `RaceDay` to **Start**.
5. Start the solution. The MVC project is configured to call the API at `https://localhost:7140/`.
6. If Visual Studio assigns a different API HTTPS port, update `RaceDay/appsettings.json` → `ApiSettings:BaseUrl` to match the API's HTTPS address.
7. Register separate accounts with the roles `Organiser` and `Participant`, then log in with each account to test the role-specific dashboards.

## Important data note
The API currently uses Entity Framework Core's **InMemory** database (`UseInMemoryDatabase("StoreDb")`). It is suitable for a local demonstration, but data is held only in memory and is cleared when the API process stops. Register accounts and create events/categories again after restarting the API. This is not durable production storage.

## Testing checklist
- Register and log in as an Organiser; verify the Organiser Dashboard, Create Event and Manage Events pages.
- Create an event while the API is running.
- Add race categories using the API category endpoint if no category-management MVC page is available yet.
- Log in as a Participant; verify Upcoming Events, Event Details, enrolment and My Enrolments.
- Record and view results only after verifying the result endpoints and the relevant enrolment IDs.

## Verification status
The source files have been inspected and several integration issues were corrected in this package. This environment does not have the .NET SDK / Visual Studio build tools, so a compile-and-run test could not be performed here. Please run **Build → Build Solution** in Visual Studio and test both account roles before submitting. Do not describe the package as independently verified until that local test succeeds.
