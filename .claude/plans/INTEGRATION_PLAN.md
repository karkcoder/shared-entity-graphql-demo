# Docker-to-App Integration & Development Workflow Plan

## Overview

This plan covers integrating the Docker PostgreSQL database with the ASP.NET Core GraphQL application, configuring Entity Framework Core with migrations, and establishing a complete development workflow.

## Part 1: Docker-to-App Integration

### 1.1 Docker Network & Connection Architecture

- [ ] Understand network setup:
  - Docker container runs on custom bridge network: `graphql-network`
  - ASP.NET app will also use same network for internal communication
  - For local development: Connect via `localhost:5432`
  - For containerized app: Connect via `postgres:5432` (service name from compose)

### 1.2 Connection String Configuration

- [ ] Create `appsettings.Development.json`:

  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Port=5432;Database=graphql_db;User Id=graphql_user;Password=your_secure_password;"
    }
  }
  ```

- [ ] Create `appsettings.Production.json`:

  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=postgres;Port=5432;Database=graphql_db;User Id=graphql_user;Password=your_secure_password;"
    }
  }
  ```

- [ ] Note the differences:
  - Development: Uses `localhost` (local machine connection)
  - Production: Uses `postgres` (Docker service name)

### 1.3 PostgreSQL NuGet Package Setup

- [ ] Install required packages:

  ```bash
  dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
  ```

- [ ] Verify package version compatibility with EntityFramework Core

### 1.4 Docker Compose with ASP.NET Service

- [ ] Update `docker-compose.yml` to include ASP.NET service:

  ```yaml
  version: "3.8"

  services:
    postgres:
      image: postgres:15
      container_name: postgres-graphql-db
      environment:
        POSTGRES_USER: graphql_user
        POSTGRES_PASSWORD: your_secure_password
        POSTGRES_DB: graphql_db
      ports:
        - "5432:5432"
      volumes:
        - ./docker/postgres-init:/docker-entrypoint-initdb.d
      networks:
        - graphql-network
      healthcheck:
        test: ["CMD-SHELL", "pg_isready -U graphql_user"]
        interval: 10s
        timeout: 5s
        retries: 5

  networks:
    graphql-network:
      driver: bridge
  ```

- [ ] Add health check to PostgreSQL to ensure database is ready before app connects

### 1.5 Verify Connectivity

- [ ] Connection string works with EF Core
- [ ] Can query PostgreSQL from application
- [ ] Database is accessible at startup

---

## Part 2: Entity Framework Core & Migrations

### 2.1 DbContext Setup

- [ ] Create `Data/AppDbContext.cs`:

  ```csharp
  using Microsoft.EntityFrameworkCore;
  using ProjectName.Models;

  namespace ProjectName.Data
  {
    public class AppDbContext : DbContext
    {
      public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

      public DbSet<State> States { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
        base.OnModelCreating(modelBuilder);

        // Additional configurations can go here
      }
    }
  }
  ```

### 2.2 Entity Model Configuration

- [ ] Create `Models/State.cs`:

  ```csharp
  using System;

  namespace ProjectName.Models
  {
    public class State
    {
      public Guid Id { get; set; }
      public string Name { get; set; }
      public string Abbreviation { get; set; }
      public DateTime CreatedAt { get; set; }
    }
  }
  ```

### 2.3 Program.cs Configuration

- [ ] Add DbContext registration in `Program.cs`:

  ```csharp
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
  builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
  ```

- [ ] Add before `builder.Build()` call

### 2.4 Entity Framework Core Tools

- [ ] Install EF Core CLI tools globally:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

- [ ] Or install locally to project:
  ```bash
  dotnet add package Microsoft.EntityFrameworkCore.Tools
  ```

### 2.5 Initial Migration (for existing database)

- [ ] Since database is pre-populated by Docker, use reverse engineering:

  ```bash
  dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=graphql_db;Username=graphql_user;Password=your_secure_password" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -f
  ```

- [ ] This creates EF Core artifacts from existing database
- [ ] Alternative: Create fresh migration after app is set up

### 2.6 Migration Strategy

- [ ] For development workflow:
  - Docker creates initial schema (state table)
  - App uses `DbContext` to read existing schema
  - Any schema changes go through EF Core migrations

- [ ] For team development:
  - Ensure all developers run same Docker setup
  - Migrations are tracked in Git
  - Run `dotnet ef database update` to apply migrations

### 2.7 Future Migrations

- [ ] When adding new tables/entities:

  ```bash
  dotnet ef migrations add AddNewEntity
  dotnet ef database update
  ```

- [ ] Migrations are stored in `Data/Migrations/` folder
- [ ] Always commit migrations to Git

---

## Part 3: Complete Development Workflow

### 3.1 Initial Setup (One-time)

- [ ] Clone repository
- [ ] Navigate to project root
- [ ] Ensure Docker Desktop is running
- [ ] Create `appsettings.Development.json` from template
- [ ] Update connection string password if needed

### 3.2 Starting Development Environment

- [ ] Start Docker PostgreSQL:

  ```bash
  docker-compose up -d postgres
  ```

- [ ] Verify database is running:

  ```bash
  docker-compose logs postgres
  ```

- [ ] Wait for health check to pass (look for "accepting connections")

- [ ] Run EF Core migrations (if applicable):

  ```bash
  dotnet ef database update
  ```

- [ ] Verify State table and data:

  ```bash
  docker-compose exec postgres psql -U graphql_user -d graphql_db -c "SELECT * FROM state ORDER BY name;"
  ```

- [ ] Build and run ASP.NET app:

  ```bash
  dotnet build
  dotnet run
  ```

- [ ] Verify GraphQL endpoint is accessible:
  - Navigate to `http://localhost:5000/graphql`
  - Should see GraphQL Playground

### 3.3 Testing GraphQL Queries

- [ ] Sample query to test:

  ```graphql
  query {
    states {
      id
      name
      abbreviation
      createdAt
    }
  }
  ```

- [ ] Should return all 50 US states

### 3.4 Stopping Development Environment

- [ ] Stop ASP.NET app:

  ```bash
  Ctrl+C (in terminal running dotnet run)
  ```

- [ ] Stop Docker containers:

  ```bash
  docker-compose down
  ```

- [ ] This removes container but keeps volumes for non-persistent setup

### 3.5 Restarting Fresh

- [ ] To reset everything and start fresh:
  ```bash
  docker-compose down
  docker-compose up -d postgres
  # Wait for health check
  dotnet ef database update
  dotnet run
  ```

### 3.6 Common Development Tasks

- [ ] **Add new database table**:
  1. Create Entity model in `Models/`
  2. Add DbSet to `AppDbContext`
  3. Create migration: `dotnet ef migrations add AddNewTable`
  4. Update database: `dotnet ef database update`

- [ ] **Modify existing table**:
  1. Update Entity model
  2. Create migration: `dotnet ef migrations add ModifyTable`
  3. Update database: `dotnet ef database update`

- [ ] **Inspect database**:

  ```bash
  docker-compose exec postgres psql -U graphql_user -d graphql_db
  ```

- [ ] **View logs**:

  ```bash
  # PostgreSQL logs
  docker-compose logs postgres

  # App logs (from terminal running dotnet run)
  ```

---

## Part 4: Environment & Configuration Files

### 4.1 appsettings Structure

- [ ] `appsettings.json` - Shared settings (no secrets)
- [ ] `appsettings.Development.json` - Local development
- [ ] `appsettings.Production.json` - Production deployment
- [ ] `appsettings.*.json` - Add to .gitignore to prevent committing secrets

### 4.2 .gitignore Entries

- [ ] Add to `.gitignore`:
  ```
  appsettings.Development.json
  appsettings.Production.json
  appsettings.*.local.json
  *.db
  bin/
  obj/
  .vs/
  .vscode/
  ```

### 4.3 Configuration Template Files

- [ ] Create `appsettings.Development.json.template`:

  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Port=5432;Database=graphql_db;User Id=graphql_user;Password=CHANGE_ME;"
    },
    "Logging": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
  ```

- [ ] Document that developers should copy template and fill in values

### 4.4 Environment Variables (Alternative)

- [ ] Instead of appsettings files, use environment variables:

  ```bash
  export ConnectionStrings__DefaultConnection="Server=localhost;Port=5432;Database=graphql_db;User Id=graphql_user;Password=your_secure_password;"
  dotnet run
  ```

- [ ] Useful for CI/CD pipelines and containerized deployments

---

## Integration Checklist

- [ ] Docker compose file includes both networking and health checks
- [ ] PostgreSQL NuGet package installed
- [ ] DbContext created and configured
- [ ] State entity model created
- [ ] Program.cs registers DbContext with PostgreSQL provider
- [ ] Connection string works for both development and production scenarios
- [ ] EF Core tools installed
- [ ] Initial database context created (via scaffold or migration)
- [ ] appsettings files created with correct connection strings
- [ ] Sensitive files added to .gitignore
- [ ] GraphQL endpoint functional
- [ ] Sample GraphQL query returns State data
- [ ] Development workflow documented and tested

---

## Workflow Diagram

```
START
  ↓
docker-compose up -d postgres
  ↓
Wait for health check (pg_isready)
  ↓
dotnet ef database update (if needed)
  ↓
dotnet run
  ↓
GraphQL API running on http://localhost:5000
  ↓
Test queries via GraphQL Playground
  ↓
dotnet ef migrations add [MigrationName] (for schema changes)
  ↓
Ctrl+C to stop app
  ↓
docker-compose down
  ↓
END
```

---

## Troubleshooting

- **Connection refused**: Database not running or port not exposed. Check `docker-compose logs postgres`
- **Migration failed**: Ensure DbContext is properly configured. Check connection string.
- **Health check failing**: PostgreSQL not ready. Wait longer or check container logs.
- **GraphQL endpoint 404**: Ensure middleware configured in Program.cs and app is running.
- **State table empty**: Verify init scripts ran. Check `01-create-tables.sql` and `02-insert-states.sql` in Docker logs.
- **Port already in use**: Change port mapping in docker-compose.yml or stop other PostgreSQL instances.
