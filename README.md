# Shared Entity GraphQL Demo

A GraphQL API built with ASP.NET Core, HotChocolate, and Entity Framework Core, providing flexible querying of US state data. The application uses PostgreSQL as the database backend and can be run locally or in Docker.

## Project Overview

This is a public GraphQL API that allows clients to:

- Query all US states
- Query individual states by ID or abbreviation
- Create new state entries
- Update existing state entries
- Delete state entries

## Technology Stack

- **Framework**: ASP.NET Core 10
- **GraphQL Server**: HotChocolate 15.1.12
- **Database ORM**: Entity Framework Core 9.0.0
- **Database**: PostgreSQL (via Npgsql)
- **Runtime**: .NET 10

## Project Structure

```
SharedEntityGraphQL/
├── Models/                 # Entity models (State)
├── Data/                   # DbContext and database configuration
├── Types/                  # GraphQL object types and schema definitions
├── Queries/                # GraphQL query resolvers
├── Mutations/              # GraphQL mutation resolvers
├── Services/               # Business logic layer
├── Migrations/             # EF Core database migrations
├── Program.cs              # Application startup and configuration
├── appsettings.json        # Production configuration
└── appsettings.Development.json  # Development configuration

docker/
├── postgres-init/          # PostgreSQL initialization scripts
│   ├── 01-create-tables.sql      # Creates state table
│   └── 02-insert-states.sql      # Seeds 50 US states

docker-compose.yml         # Docker Compose configuration
```

## Getting Started

### Prerequisites

- .NET 8 SDK or later
- Docker and Docker Compose (for containerized PostgreSQL)
- PostgreSQL 12+ (if running locally without Docker)

### Local Development Setup

#### 1. Install Dependencies

All NuGet packages are already configured in the project file.

#### 2. Start PostgreSQL with Docker

```bash
docker-compose up -d
```

This starts a PostgreSQL container on port 5432 with:

- Username: `graphql_user`
- Password: `graphql_password`
- Database: `graphql_db`

#### 3. Build and Run the Application

```bash
cd SharedEntityGraphQL
dotnet build
dotnet run --launch-profile https
```

The application will start on `https://localhost:5001` and `http://localhost:5000`.

#### 4. Access GraphQL Playground

Once the application is running, access the GraphQL Playground/Apollo Studio at:

```
http://localhost:5000/graphql
```

### Environment Configuration

#### Development (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=graphql_db;User Id=graphql_user;Password=graphql_password;"
  }
}
```

Uses `localhost` for local PostgreSQL connections.

#### Production (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=postgres;Port=5432;Database=graphql_db;User Id=graphql_user;Password=graphql_password;"
  }
}
```

Uses Docker service name `postgres` for containerized deployments.

## Database

### Schema

The application uses a single `state` table with the following structure:

```sql
CREATE TABLE state (
    id UUID PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    abbreviation VARCHAR(2) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Migrations

Entity Framework Core migrations are automatically applied on application startup. To manage migrations:

```bash
# Add a new migration
dotnet-ef migrations add MigrationName

# Apply migrations to database
dotnet-ef database update

# Remove the last migration
dotnet-ef migrations remove
```

### Initial Data

The database is seeded with all 50 US states automatically when the Docker container starts. This is done via:

- `docker/postgres-init/01-create-tables.sql` - Creates the schema
- `docker/postgres-init/02-insert-states.sql` - Seeds the states

## GraphQL API

### Queries

#### Get All States

```graphql
query {
  getStates {
    id
    name
    abbreviation
    createdAt
  }
}
```

#### Get State by ID

```graphql
query {
  getStateById(id: "UUID-HERE") {
    id
    name
    abbreviation
    createdAt
  }
}
```

#### Get State by Abbreviation

```graphql
query {
  getStateByAbbreviation(abbreviation: "CA") {
    id
    name
    abbreviation
    createdAt
  }
}
```

### Mutations

#### Create a State

```graphql
mutation {
  createState(name: "NewState", abbreviation: "NS") {
    id
    name
    abbreviation
    createdAt
  }
}
```

#### Update a State

```graphql
mutation {
  updateState(id: "UUID-HERE", name: "UpdatedName", abbreviation: "UN") {
    id
    name
    abbreviation
    createdAt
  }
}
```

#### Delete a State

```graphql
mutation {
  deleteState(id: "UUID-HERE")
}
```

## Docker Deployment

### Building the Application Image

Create a `Dockerfile` in the project root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "SharedEntityGraphQL.dll"]
```

### Running with Docker Compose

The `docker-compose.yml` file is already configured to run PostgreSQL. To add the ASP.NET application:

```yaml
services:
  app:
    build: ./SharedEntityGraphQL
    ports:
      - "5000:80"
      - "5001:443"
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    networks:
      - graphql-network
```

## Development Workflow

### Adding a New Entity

1. **Create the Model** in `Models/YourEntity.cs`
2. **Add DbSet** to `ApplicationDbContext`
3. **Create GraphQL Type** in `Types/YourEntityType.cs`
4. **Create Service** in `Services/YourEntityService.cs`
5. **Add Queries** in `Queries/Query.cs`
6. **Add Mutations** in `Mutations/Mutation.cs` (if needed)
7. **Register Service** in `Program.cs`
8. **Create Migration**: `dotnet-ef migrations add AddYourEntity`
9. **Update Configuration** in `Program.cs` if needed

### Database Migrations

When you modify the database schema:

```bash
# Create a migration
dotnet-ef migrations add DescripteChangeHere

# Apply to database (happens automatically on app start)
dotnet-ef database update
```

## Troubleshooting

### Connection Issues

If you get connection errors:

1. Verify PostgreSQL is running:

   ```bash
   docker-compose ps
   docker-compose logs postgres
   ```

2. Check connection string in `appsettings.Development.json`

3. Ensure EF Core migrations are applied (happens automatically on startup)

### Build Issues

1. Clean and rebuild:

   ```bash
   dotnet clean
   dotnet build
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
  - Default: `Development` (local development)
  - Use `Production` for Docker deployments

### Secrets Management

For sensitive data (passwords, API keys):

1. **Local Development**: Use `appsettings.Development.json` (excluded from git)
2. **Docker**: Set environment variables in `docker-compose.yml`
3. **Production**: Use environment variables or Azure Key Vault

## Performance Considerations

- Indexes on `name` and `abbreviation` fields for fast lookups
- Connection pooling enabled via Npgsql
- GraphQL query execution optimized by HotChocolate
- Async/await throughout for non-blocking operations

## Security Notes

- API is currently public (no authentication)
- To add authentication, implement JWT or API key validation in middleware
- Connection strings should not be committed to version control
- Use strong passwords in production environments

## Future Enhancements

- [ ] Add authentication/authorization
- [ ] Implement role-based access control (RBAC)
- [ ] Add query filtering and pagination
- [ ] Implement caching strategies
- [ ] Add subscription support
- [ ] Create unit and integration tests
- [ ] Add API rate limiting
- [ ] Implement comprehensive logging

## Contributing

1. Create a feature branch
2. Make your changes
3. Ensure all tests pass
4. Submit a pull request

## License

This project is provided as-is for demonstration purposes.

## Support

For issues or questions:

1. Check the troubleshooting section
2. Review the GraphQL schema in the playground
3. Check application logs for error details
