# Commit 6: MVC foundation

## Upload

Extract the ZIP. On the part-3 branch upload the **RaceDay.Web folder itself**, plus the root POE_Part2.csproj, POE_Part2.sln and this guide. Drag the folder into GitHub's upload area to preserve its subfolders. Do not flatten its files into the API root and do not upload the ZIP.

Before committing confirm paths include RaceDay.Web/Controllers/HomeController.cs and RaceDay.Web/Views/Home/Index.cshtml. Replace the two existing root project/solution files.

Commit message: `feat: add RaceDay MVC project and responsive home page`

## Run locally

Install the .NET 8 SDK. From the extracted repository folder:

```powershell
dotnet restore POE_Part2.sln
dotnet build POE_Part2.sln --configuration Release
dotnet dev-certs https --trust
dotnet run --project RaceDay.Web/RaceDay.Web.csproj
```

Open https://localhost:7101. Alternatively open the solution in Visual Studio and set RaceDay.Web as the startup project. Check the home page on a narrow window and use the keyboard to navigate the links.

The static home page runs without SQL Server or the API. The named RaceDayApi HTTP client is registered for subsequent features, but no data calls are implemented yet. Api:BaseUrl in RaceDay.Web/appsettings.json is a placeholder: change it to your API's actual URL from its startup console before adding data features. Never put database or Azure credentials in the MVC project.

## What changed

- Separate .NET 8 MVC application with controller, Razor views and shared layout.
- Responsive RaceDay styling and an accessible skip link.
- Friendly production exception page.
- Configurable API HTTP client, with no Entity Framework or database dependency.
- Solution includes both projects. API project excludes the nested MVC folder from its default file glob.

An existing workflow that builds POE_Part2.sln will now build both projects. If your workflow is still GitHub's sample echo workflow, it must be corrected; a green echo job does not compile either project.

This is the foundation only: registration, session authentication, role menus and real event pages come in subsequent commits. The CSS is local; a CSS framework can be integrated with the later UI work.

Preparation checks: project XML, JSON settings, solution membership and package paths checked. No .NET SDK was available in the preparation environment, so compilation and browser execution must be verified locally or in the actual build workflow.
