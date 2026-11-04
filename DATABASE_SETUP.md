# SQL Server persistence — Part 3

This step switches the API from temporary in-memory data to SQL Server. It uses the current Part 3 models, including event descriptions, image URLs and enrolment status. It does not add MVC or complete authentication yet.

## Upload this commit

On the **part-3** branch, upload these four files at the repository root (beside POE_Part2.csproj). Replace Program.cs, appsettings.json and SQLQuery1.sql; add DATABASE_SETUP.md. Upload extracted files, not the ZIP or its containing folder.

Commit message: `feat: persist RaceDay API data in SQL Server`

## Set up locally

1. Install SQL Server Express LocalDB through Visual Studio Installer (Data storage and processing), and SQL Server Management Studio (SSMS).
2. In SSMS connect to `(localdb)\MSSQLLocalDB` using Windows Authentication.
3. Open the **new** SQLQuery1.sql from this commit and execute the entire script. It creates a separate database named RaceDayPart3 with six empty tables. Do not use the older Part 2 script.
4. Open POE_Part2.sln in Visual Studio and run the API. Alternatively run `dotnet run --project POE_Part2.csproj` from the repository folder.
5. Use Swagger in Development to register a disposable test user. Stop and restart the API, then confirm that user still exists in the Users table. Do not use a real password: password hashing is a later step.

The setup script refuses to run when RaceDayPart3 already contains any user tables. That is expected after successful setup; do not delete data to rerun it. This script is an initial schema, not an upgrade/migration for an existing database. Existing Part 2 databases remain untouched. Previous in-memory records cannot be recovered after shutdown.

## If you use SQL Express instead

Connect SSMS to your actual SQL Server instance and execute the script there. Set the API connection string to that same instance. For example, in PowerShell before `dotnet run`:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=RaceDayPart3;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project POE_Part2.csproj
```

The appsettings.json default is Windows LocalDB. LocalDB does not run inside Linux/Docker; later Docker setup must use a reachable SQL Server host. TrustServerCertificate=True is for local development. For hosted SQL Server use the provider's encrypted connection settings and keep credentials in environment variables or user secrets, never in GitHub.

If the API reports a connection error, verify the instance name, that its service is running, and that RaceDayPart3 exists there. If it reports an invalid object/table, execute the new setup script against the same server configured in the API. The API intentionally does not create/drop databases automatically.

## Current limitations and checks

Foreign keys now enforce real relationships: an event needs an existing organiser and venue; an enrolment needs a participant and category. API validation and organiser workflows will be improved in later commits. No seed accounts/passwords are included.

The schema's columns, nullability, decimal precision and six foreign keys were checked against the current models/context. Compilation and SQL execution were not performed in the preparation environment. Run the local persistence check above. A green GitHub Actions build verifies compilation only; it does not prove SQL connectivity or the full application works.
