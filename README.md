# Playlist API - Academy Backend Developer Test

**Author:** Belal Nader
**Framework:** ASP.NET Core 10 Web API
**Database:** SQLite via Entity Framework Core

---

## 🚀 Phase 1: Local Setup & Database Generation

This project is built to run easily on any machine with zero external database installation required.

### Prerequisites
1. Install the .NET 10 SDK (https://dotnet.microsoft.com/download/dotnet/10.0).
2. Open the root folder in your preferred code editor (e.g., Visual Studio Code).

### Database Initialization
Before running the app, we need to let Entity Framework generate the local SQLite database file.
1. Open your terminal and navigate into the main API folder:
   ```bash
   cd PlaylistAPI
   ```
2. Build the project to restore all dependencies:
   ```bash
   dotnet build
   ```
3. Execute the database migrations to build the playlist.db file and its tables:
   ```bash
   dotnet ef database update
   ```

---

## 💻 Phase 2: Running the API

Once the database is initialized, you can launch the local server.
1. Make sure your terminal is still inside the PlaylistAPI directory.
2. Run the server using the HTTP profile:
   ```bash
   dotnet run --launch-profile "http"
   ```
3. Open your browser and navigate to the interactive Swagger dashboard to test the endpoints:
   http://localhost:5284/swagger

---

## 🧪 Phase 3: Executing the Test Suite (Bonus Deliverable)

The testing architecture is split into a separate project to ensure the production database is never corrupted. It contains both Unit Tests and Integration Tests.

### How to Run the Tests
1. Open a new terminal and navigate to the test project directory:
   ```bash
   cd PlaylistAPI.Tests
   ```
2. Run the complete suite:
   ```bash
   dotnet test
   ```

### What the Tests Are Checking:
* Unit Tests (In-Memory DB): These tests isolate the PlaylistController and inject a fake RAM-based database to verify the core C# logic (e.g., returning an empty list when the DB is empty, or rejecting invalid IDs).
* Integration Tests (SQLite Sandbox): These tests boot up a full simulation of the web server. To protect your real data, the suite automatically creates a disposable integration_test.db file, tests the physical saving/updating/deleting of HTTP requests, and verifies that the [Required] data validation attributes actively block bad user inputs.

---

## Database Architecture & Justification

### Why SQLite?
For this specific assessment, SQLite was chosen over a heavier SQL database (like PostgreSQL) or a NoSQL database (like MongoDB) for two critical reasons:
1. Zero-Configuration Portability: The prompt requires the code to run properly on any machine. SQLite creates a local .db file directly inside the project folder, eliminating the need for reviewers to install, configure, or authenticate an external database server just to run the code.
2. Relational Data Integrity: The relationship between a Playlist and its Songs is strictly relational (One-to-Many). A SQL relational database natively handles these Foreign Key constraints.

### Database Schema (Entity-Relationship)
Table: Playlists
* Id (PK, Auto-Incrementing Integer)
* Name (String, Required, Max Length: 50)
* Description (String, Required, Max Length: 500)

Table: Songs
* Id (PK, Auto-Incrementing Integer)
* Title (String, Required, Max Length: 100)
* Artist (String, Required)
* DurationInSeconds (Integer, Required, Range: 1-3600)
* PlaylistId (FK, links to Playlists.Id)

---

## Architectural Decisions & SOLID Principles

* Data Transfer Objects (DTOs): The API never exposes raw database models to the client. DTOs act as messengers for all inputs and outputs to ensure security and adhere to the Single Responsibility Principle.
* Dependency Injection: The database context is injected into the controller via the constructor, adhering to the Dependency Inversion Principle. This is what allowed the Integration Tests to seamlessly swap the production database for a sandbox file.