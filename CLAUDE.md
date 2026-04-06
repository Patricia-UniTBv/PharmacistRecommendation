# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Recomandarea Farmacistului** — a .NET 8 MAUI Windows desktop application for pharmacy patient management, medication monitoring, and pharmacist recommendations. The UI is in Romanian.

## Build & Run Commands

```bash
# Build solution
dotnet build PharmacistRecommendation.sln

# Run the application
dotnet run --project PharmacistRecommendation/PharmacistRecommendation.csproj

# Publish for server deployment (Windows Auth, local SQL Server)
dotnet publish PharmacistRecommendation/PharmacistRecommendation.csproj /p:PublishProfile=Server

# Publish for client deployment (SQL Auth, remote SQL Server)
dotnet publish PharmacistRecommendation/PharmacistRecommendation.csproj /p:PublishProfile=Client
```

The SDK version is pinned to 8.0.416 via `global.json` (`rollForward: disable`).

## Entity Framework Migrations

Migrations live in `Entities/Migrations/`. Target project is always `Entities`:

```bash
dotnet ef migrations add <MigrationName> -p Entities
dotnet ef database update -p Entities
dotnet ef migrations remove -p Entities
```

## Solution Structure

Three projects with a clear dependency chain: `PharmacistRecommendation` → `Entities` → `DTO`

| Project | Type | Role |
|---|---|---|
| `PharmacistRecommendation` | .NET 8 MAUI (Windows) | Presentation layer: Views, ViewModels, app services |
| `Entities` | .NET 8 Class Library | Domain: Models, Repositories, Services, EF Core context |
| `DTO` | .NET 8 Class Library | Data transfer objects (no dependencies) |

## Architecture

**Pattern:** N-Tier + MVVM

```
Views (XAML)
  └─ ViewModels (CommunityToolkit.Mvvm ObservableObject + RelayCommand)
       └─ Services (Entities.Services — business logic)
            └─ Repositories (Entities.Repository — EF Core abstraction)
                 └─ PharmacistRecommendationDbContext (SQL Server)
```

**Dependency injection** is configured in `MauiProgram.cs`. Most services are **transient** to align with DbContext lifetime. `AuthenticationService` is singleton (maintains session state; be aware of captive dependency risks if it depends on transient services).

**Session state** is managed by `PharmacistRecommendation/Helpers/SessionManager.cs`.

**Configuration** is loaded by `PharmacistRecommendation/Helpers/ConfigurationManager.cs`:
1. `appsettings.json` embedded in the app (sets `DeploymentMode`)
2. User config at `C:\ProgramData\PharmacistRecommendation\config.json` (runtime overrides)

## Deployment Modes

The application has two publish profiles and matching settings files:

- **Server** (`appsettings.Server.json`): Windows Integrated Auth, local `localhost\PHARMACYREC` SQL Server instance.
- **Client** (`appsettings.Client.json`): SQL Auth (`appuser`), remote server address entered on first run.

First-run detection and setup wizards are handled by `FirstRunHelper.cs`. The wizard creates the database (Server mode) or configures the remote connection (Client mode).

## Key Namespaces

- `PharmacistRecommendation.Views` — 21 XAML pages
- `PharmacistRecommendation.ViewModels` — 20 view models
- `PharmacistRecommendation.Helpers` — `ConfigurationManager`, `SessionManager`, `FirstRunHelper`, `PdfReportService`, `EmailSenderService`
- `Entities.Models` — 22 domain entity classes
- `Entities.Repository` / `Entities.Repository.Interfaces` — 11 repositories
- `Entities.Services` / `Entities.Services.Interfaces` — 16 services
- `Entities.Parameters` — typed monitoring parameter objects (Cardio, Diabetes, Temperature), serialized as JSON in the database
- `DTO` — DTOs for monitoring variants, users, pharmacist, pharmacy card, history rows

## Database

- **Database name:** `PharmacistRecommendationDB`
- **Engine:** SQL Server 2022 Express, named instance `PHARMACYREC`
- Monitoring parameters (`CardioMonitoring`, `DiabetesMonitoring`, `TemperatureMonitoring`) are stored as JSON columns, deserialized into `Entities.Parameters` types.
- A `.bacpac` backup file is included for deployment seeding.

## Testing

There are no automated test projects. Manual testing procedures are documented in:
- `PharmacistRecommendation/TESTING_FIRST_RUN.md` — first-run wizard test scenarios
- `PharmacistRecommendation/FIRST_RUN_SETUP_GUIDE.md` — setup wizard integration guide

To test the Server first-run wizard: drop `PharmacistRecommendationDB` in SSMS, then run the app. To test the Client first-run wizard: delete `C:\ProgramData\PharmacistRecommendation\config.json` and set `"DeploymentMode": "Client"` in `appsettings.json`.

## Key Third-Party Libraries

| Library | Purpose |
|---|---|
| `CommunityToolkit.Mvvm` | MVVM (ObservableObject, RelayCommand, ObservableProperty) |
| `CommunityToolkit.Maui` | Extended MAUI UI controls |
| `Microsoft.EntityFrameworkCore.SqlServer` v9 | ORM |
| `QuestPDF` + `PDFsharp` | PDF report generation |
| `ClosedXML` | Excel export |
| `MailKit` / `MimeKit` | Email sending |
| `BCrypt.Net-Next` | Password hashing |
| `ScottPlot` | Charting |
