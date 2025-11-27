# Start Local PostgreSQL Database for Testing

## Option 1: Using Docker Compose (Recommended)

1. **Make sure Docker Desktop is running** (check system tray)

2. **Start the database:**
   ```bash
   docker-compose up -d
   ```

3. **Wait for initialization (first time only):**
   ```bash
   docker-compose logs -f postgres
   # Wait until you see: "database system is ready to accept connections"
   # Press Ctrl+C to exit logs
   ```

4. **Verify it's running:**
   ```bash
   docker-compose ps
   ```

5. **The database is now ready!**
   - Host: `localhost`
   - Port: `5432`
   - Database: `exiobase`
   - Username: `postgres`
   - Password: `password`

## Option 2: Using Docker Run

```bash
# Start PostgreSQL
docker run --name trade-postgres \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=exiobase \
  -p 5432:5432 \
  -d postgres:16

# Run migrations
cd DbMigrate
dotnet run
```

## Stopping the Database

```bash
# Stop but keep data
docker-compose stop

# Stop and remove (data is preserved in volume)
docker-compose down

# Remove everything including data
docker-compose down -v
```

## Checking Database Status

```bash
# View logs
docker-compose logs postgres

# Check if healthy
docker-compose ps

# Connect to database
docker exec -it trade-postgres psql -U postgres -d exiobase
```

## Running Migrations

Once the database is running:

```bash
cd DbMigrate
dotnet run
```

This will:
- Create all 9 tables (trade, trade_employment, trade_factor, etc.)
- Create indexes for performance
- Set up triggers for updated_at columns

## Testing the Application

After migrations are complete:

```bash
cd ModelEarth
dotnet run
# Navigate to: http://localhost:5094/TradeImport
```
