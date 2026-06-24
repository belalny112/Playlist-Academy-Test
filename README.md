# Playlist API - Academy Backend Developer Test

**Author:** Belal Nader  
**Framework:** ASP.NET Core 10 Web API  
**Database:** SQLite via Entity Framework Core

---

## 🚀 Phase 1: Local Setup & Database Generation

This project runs on any machine with zero external database installation required.

### Prerequisites
1. Install the .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0
2. Open the root folder in your preferred editor (e.g., Visual Studio Code).

### Database Initialization
1. Navigate into the main API folder:
   ```bash
   cd PlaylistAPI
   ```
2. Build the project to restore all dependencies:
   ```bash
   dotnet build
   ```
3. Apply all migrations to create the `playlist.db` file and its tables:
   ```bash
   dotnet ef migrations add AddPlaylistIdToSong
   dotnet ef database update
   ```

> **Note:** You may see a `NU1903` warning about a known vulnerability in `SQLitePCLRaw.lib.e_sqlite3 2.1.11`. This is a transitive dependency pulled in by `Microsoft.EntityFrameworkCore.Sqlite` and cannot be upgraded independently — it does not affect the functionality of this project.

---

## 💻 Phase 2: Running the API

1. Ensure your terminal is inside the `PlaylistAPI` directory.
2. Run the server:
   ```bash
   dotnet run --launch-profile "http"
   ```
3. Open the Swagger UI in your browser to explore and test all endpoints:  
   http://localhost:5284/swagger

---

## 🧪 Phase 3: Running the Test Suite

The test project is kept separate to ensure the production database is never touched.

1. Navigate to the test project:
   ```bash
   cd PlaylistAPI.Tests
   ```
2. Run all tests:
   ```bash
   dotnet test
   ```

### What the Tests Cover
- **Unit Tests** (In-Memory DB): Isolate the `PlaylistController` and `PlaylistService`, injecting a RAM-based database to verify core logic — empty list returns, valid creation, 404 on missing IDs, song addition.
- **Integration Tests** (SQLite Sandbox): Boot a full web server simulation. Each test receives its own isolated SQLite file (named by GUID) so tests are safe to run in parallel. Tests verify end-to-end HTTP flows: create, get, update, delete, and song addition, as well as validation rejection (missing name, invalid duration).

---

## Database Architecture & Justification

### Why SQLite?
1. **Zero-Configuration Portability:** SQLite creates a local `.db` file inside the project, so reviewers need no external database server to run the code.
2. **Relational Data Integrity:** The Playlist → Songs relationship is strictly One-to-Many. A relational database enforces this naturally with Foreign Key constraints.

### Schema (Entity-Relationship)

**Table: Playlists**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, Auto-Increment |
| Name | TEXT | Required, Max 50 chars |
| Description | TEXT | Optional, Max 500 chars |

**Table: Songs**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INTEGER | PK, Auto-Increment |
| Title | TEXT | Required, Max 100 chars |
| Artist | TEXT | Required |
| DurationInSeconds | INTEGER | Required, Range: 1–3600 |
| PlaylistId | INTEGER | FK → Playlists.Id |

---

## Architecture & SOLID Principles

### Project Structure
```
PlaylistAPI/
├── Controllers/        # HTTP layer only — routes requests, returns responses
│   └── PlaylistController.cs
├── Services/           # Business logic layer
│   ├── IPlaylistService.cs
│   └── PlaylistService.cs
├── DTOs/               # Data Transfer Objects (input/output contracts)
│   ├── CreatePlaylistDto.cs
│   ├── CreateSongDto.cs
│   ├── PlaylistDto.cs
│   └── SongDto.cs
├── Models/             # EF Core database entities
│   ├── Playlist.cs
│   └── Song.cs
├── Data/
│   └── PlaylistDbContext.cs
└── Program.cs

PlaylistAPI.Tests/
├── PlaylistControllerTests.cs   # Unit tests
└── PlaylistIntegrationTests.cs  # Integration tests
```

### SOLID Application

- **Single Responsibility:** The controller only handles HTTP concerns. All business logic lives in `PlaylistService`. DTO mapping is centralized in a single private `MapToDto` method inside the service.
- **Open/Closed:** New operations can be added by extending `IPlaylistService` without modifying the controller.
- **Dependency Inversion:** `PlaylistController` depends on the `IPlaylistService` abstraction, not the concrete `PlaylistService`. This is what allows unit tests to inject a real service with an in-memory DB, and integration tests to swap the DB connection entirely.
