# Docker PostgreSQL Setup Plan

## Overview

Set up a Docker PostgreSQL database container that automatically creates a `state` table with GUID primary key and populates it with US state data on startup. The volume does not need to be persistent since this is a development/testing database.

## Setup Steps

### Phase 1: Docker & Container Configuration

- [ ] Create `docker-compose.yml` in project root
- [ ] Configure PostgreSQL service:
  - Image: `postgres:latest` (or specific version)
  - Container name: `postgres-graphql-db`
  - Port mapping: `5432:5432` (local:container)
  - Non-persistent volume: Use `tmpfs` or temporary volume
  - Environment variables:
    - `POSTGRES_USER`: Set username (e.g., `graphql_user`)
    - `POSTGRES_PASSWORD`: Set password
    - `POSTGRES_DB`: Set database name (e.g., `graphql_db`)

### Phase 2: Database Initialization Scripts

- [ ] Create `docker/postgres-init/` directory
- [ ] Create `01-create-tables.sql` script:
  - Create `state` table with GUID primary key
  - Table structure:
    ```sql
    CREATE TABLE state (
      id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
      name VARCHAR(50) NOT NULL UNIQUE,
      abbreviation VARCHAR(2) NOT NULL UNIQUE,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );
    ```
- [ ] Create `02-insert-states.sql` script:
  - INSERT statements for all 50 US states with abbreviations
  - Include states in alphabetical order

### Phase 3: Docker Entry Point Configuration

- [ ] Configure docker-compose to mount init scripts:
  - Mount `docker/postgres-init/` to `/docker-entrypoint-initdb.d/`
  - PostgreSQL automatically runs all `.sql` files in this directory on first startup
- [ ] Ensure scripts run in correct order by naming conventions (01-, 02-, etc.)

### Phase 4: Connection & Verification

- [ ] Document connection string for application:
  - Format: `Server=localhost;Port=5432;Database=graphql_db;User Id=graphql_user;Password={password};`
- [ ] Verify table exists by connecting to container
- [ ] Verify all 50 states are populated

### Phase 5: Docker Compose Commands Documentation

- [ ] Document common commands:
  - `docker-compose up -d` - Start container in background
  - `docker-compose down` - Stop and remove container
  - `docker-compose logs postgres` - View container logs
  - `docker-compose exec postgres psql -U graphql_user -d graphql_db` - Access database shell

## File Structure

```
project-root/
├── docker-compose.yml
├── docker/
│   └── postgres-init/
│       ├── 01-create-tables.sql
│       └── 02-insert-states.sql
└── [other project files]
```

## docker-compose.yml Template

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
      - /dev/shm:/dev/shm # Use tmpfs for no persistence
    networks:
      - graphql-network

networks:
  graphql-network:
    driver: bridge
```

## 01-create-tables.sql Template

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE state (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(50) NOT NULL UNIQUE,
  abbreviation VARCHAR(2) NOT NULL UNIQUE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_state_name ON state(name);
CREATE INDEX idx_state_abbreviation ON state(abbreviation);
```

## 02-insert-states.sql Template

```sql
INSERT INTO state (name, abbreviation) VALUES
('Alabama', 'AL'),
('Alaska', 'AK'),
('Arizona', 'AZ'),
('Arkansas', 'AR'),
('California', 'CA'),
-- ... (all 50 states)
('Wyoming', 'WY');
```

## Configuration Checklist

- [ ] docker-compose.yml created and configured
- [ ] Init scripts directory created
- [ ] Schema creation script completed
- [ ] State data insertion script completed
- [ ] Container builds without errors
- [ ] Database initializes on first `docker-compose up`
- [ ] All 50 states present in database
- [ ] Connection string working from application
- [ ] Container can be easily torn down and recreated

## Development Notes

- Non-persistent volume means data is lost when container stops
- Useful for development/testing as data resets on container restart
- To use persistent volume, change to standard Docker volume mounting
- Can scale to other tables as needed
- Init scripts are idempotent (safe to run multiple times with proper structure)
- Ensure UUID extension is available in init script

## Troubleshooting

- If database doesn't initialize: Check Docker logs with `docker-compose logs postgres`
- If connection fails: Verify port mapping and container is running
- If states not populated: Check if SQL scripts are in correct directory and have no syntax errors
- To reset: `docker-compose down` then `docker-compose up`
