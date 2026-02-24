# Quick Start Guide

## 5-Minute Setup

### 1. Start PostgreSQL Docker Container

```bash
docker-compose up -d
```

Wait for PostgreSQL to be healthy:

```bash
docker-compose logs postgres
```

### 2. Build the Application

```bash
cd SharedEntityGraphQL
dotnet build
```

### 3. Run the Application

```bash
dotnet run --launch-profile https
```

### 4. Test the API

Open your browser and navigate to:

```
http://localhost:5000/graphql
```

You should see the GraphQL Playground/Apollo Studio interface.

## First Query

Paste this into the GraphQL Playground and click the "Play" button:

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

You should see a list of all 50 US states!

## Common Commands

### View PostgreSQL Logs

```bash
docker-compose logs -f postgres
```

### Stop PostgreSQL

```bash
docker-compose down
```

### Rebuild Project

```bash
dotnet build --force
```

### View Database Migrations

```bash
dotnet-ef migrations list
```

## Troubleshooting

### "Connection timeout" errors

- Ensure Docker is running: `docker ps`
- Check if PostgreSQL is healthy: `docker-compose ps`
- Wait a few seconds for PostgreSQL to fully initialize

### "Column does not exist" errors

- Migrations are applied automatically on startup
- If issues persist, check the logs: `docker-compose logs postgres`

### GraphQL endpoint not available

- Ensure the application is running: `dotnet run`
- Check that it's on port 5000: `http://localhost:5000`
- The endpoint is at `/graphql`

## Next Steps

1. **Explore the GraphQL Schema**: Click the "Schema" tab in Apollo Studio
2. **Read the Docs**: Check [README.md](README.md) for full documentation
3. **Add Your Own Entities**: Follow the guide in the README
4. **Deploy to Docker**: Build a container image for your application

## Architecture Overview

```
Client (GraphQL)
        ↓
    GraphQL Server (HotChocolate)
        ↓
    Services (Business Logic)
        ↓
    Entity Framework Core
        ↓
    PostgreSQL Database
```

## Project Layout

- **Models**: C# classes that represent database entities
- **Data**: EF Core DbContext and migrations
- **Types**: GraphQL type definitions
- **Queries**: Read operations in GraphQL
- **Mutations**: Write operations in GraphQL
- **Services**: Business logic and data access

## Helpful Tips

1. **GraphQL Introspection**: The API automatically provides schema information to clients
2. **Auto-Migrations**: Database migrations run automatically on app startup
3. **Hot Reload**: Use `dotnet watch run` for automatic restarts during development
4. **Environment-Specific Config**: Development settings in `appsettings.Development.json`

## Example Queries

### Create a State

```graphql
mutation {
  createState(name: "Test State", abbreviation: "TS") {
    id
    name
    abbreviation
  }
}
```

### Update a State

```graphql
mutation {
  updateState(id: "GUID-HERE", name: "Updated") {
    id
    name
  }
}
```

### Query by Abbreviation

```graphql
query {
  getStateByAbbreviation(abbreviation: "CA") {
    id
    name
    abbreviation
  }
}
```

## Getting Help

1. Check the [GraphQL Documentation](https://graphql.org)
2. Review [HotChocolate Documentation](https://chillicream.com/docs/hotchocolate)
3. Check the [README.md](README.md) for detailed information
4. Review the application logs: `dotnet run` (will show errors in console)

---

Happy coding! 🚀
