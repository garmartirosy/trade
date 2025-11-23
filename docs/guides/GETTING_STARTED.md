# Getting Started with Trade Data Import System

## Table of Contents
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [First Import](#first-import)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

```
┌──────────────────────────────────────────────┐
│ Software Requirements                         │
├──────────────────────────────────────────────┤
│ .NET 8.0 SDK             ✓ Required         │
│ PostgreSQL 12+           ✓ Required         │
│ Git                      ✓ Required         │
│ Code Editor (VS/VS Code) ○ Recommended     │
└──────────────────────────────────────────────┘
```

### Installation Links

- **.NET 8.0 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **PostgreSQL**: https://www.postgresql.org/download/
- **Git**: https://git-scm.com/downloads
- **Visual Studio Code**: https://code.visualstudio.com/

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should output: 8.0.x

# Check PostgreSQL
psql --version
# Should output: psql (PostgreSQL) 12.x or higher

# Check Git
git --version
# Should output: git version 2.x
```

---

## Installation

### Step-by-Step Installation

```
┌────────────────────────────────────────────────────┐
│ Installation Flow                                   │
│                                                     │
│  1. Clone Repository                               │
│     ↓                                               │
│  2. Setup Database                                  │
│     ↓                                               │
│  3. Configure Environment                           │
│     ↓                                               │
│  4. Restore Dependencies                            │
│     ↓                                               │
│  5. Run Application                                 │
└────────────────────────────────────────────────────┘
```

### 1. Clone the Repository

```bash
# Clone from GitHub
git clone https://github.com/modelearth/trade.git

# Navigate to project directory
cd trade

# View project structure
ls
# Output:
# ModelEarth/
# ModelEarth.Tests/
# docs/
# README.md
# documentation.md
# .env.example
```

### 2. Get Trade Data Files

```bash
# Clone the trade-data repository (contains CSV files)
cd ..
git clone https://github.com/ModelEarth/trade-data.git

# Verify CSV files exist
ls trade-data/year/2022/US/imports/
# Should show: trade.csv, trade_employment.csv, etc.

# Go back to trade project
cd trade
```

### 3. Setup PostgreSQL Database

#### Option A: Local PostgreSQL

```bash
# Create database
createdb exiobase

# Or using psql
psql -U postgres
CREATE DATABASE exiobase;
\q
```

#### Option B: Azure PostgreSQL

```
1. Go to Azure Portal: https://portal.azure.com
2. Create PostgreSQL Flexible Server
3. Set server name: modelearth-postgres-server
4. Set admin username: postgresadmin
5. Set admin password: [your-secure-password]
6. Create database: exiobase
7. Configure firewall: Add your IP address
```

### 4. Run Database Migrations

```bash
# Connect to your database
psql -U postgres -d exiobase

# Run migration scripts in order
\i ModelEarth/DB\ Scripts/Postgres/001_CreateTradeTable.sql
\i ModelEarth/DB\ Scripts/Postgres/002_CreateAdditionalTables.sql
\i ModelEarth/DB\ Scripts/Postgres/003_CreateStoredProcs.sql

# Verify tables were created
\dt public.*
# Should show 9 tables: trade, trade_employment, trade_factor, etc.

# Exit psql
\q
```

---

## Configuration

### Create .env File

```bash
# Copy example configuration
cp .env.example .env

# Edit .env file
nano .env  # or use your preferred editor
```

### .env Configuration

```env
# ============================================
# Database Configuration
# ============================================
# For local PostgreSQL:
DATABASE_HOST=localhost
DATABASE_NAME=exiobase
DATABASE_USER=postgres
DATABASE_PASSWORD=your_local_password
DATABASE_PORT=5432

# For Azure PostgreSQL:
# DATABASE_HOST=modelearth-postgres-server.postgres.database.azure.com
# DATABASE_NAME=exiobase
# DATABASE_USER=postgresadmin
# DATABASE_PASSWORD=your_azure_password
# DATABASE_PORT=5432

# ============================================
# Trade Data Path
# ============================================
# Path to trade-data repository (relative or absolute)
TRADE_DATA_REPO_PATH=../trade-data

# ============================================
# Performance Settings
# ============================================
# Number of records per batch insert
BATCH_SIZE=1000
```

### Configuration Validation

```bash
# Test connection string
psql -h localhost -U postgres -d exiobase -c "SELECT version();"

# Verify trade-data path
ls ../trade-data/year/2022/
# Should show country folders: US, IN, CN, etc.
```

---

## Running the Application

### Build and Run

```bash
# Navigate to application directory
cd ModelEarth

# Restore NuGet packages
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

### Expected Output

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5094
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\...\trade\ModelEarth
```

### Access the Application

```
1. Open your browser
2. Navigate to: http://localhost:5094/TradeImport
3. You should see the import interface
```

### Application Interface

```
┌──────────────────────────────────────────────────────────┐
│  Trade Data Import                                        │
│                                                           │
│  Import CSV trade data files into the PostgreSQL database│
│                                                           │
│  ┌─────────────────────────────────────────────────────┐│
│  │ Import Configuration                                 ││
│  │                                                       ││
│  │  Year:  [2019 ▼]  [2022 ▼]                          ││
│  │                                                       ││
│  │  Countries (Optional):  [____________]               ││
│  │  (Leave blank for all countries)                     ││
│  │                                                       ││
│  │  ☐ Clear existing data for this year before import  ││
│  │                                                       ││
│  │  [ Create Database ]  [ Test Connection ]           ││
│  └─────────────────────────────────────────────────────┘│
│                                                           │
│  ┌─────────────────────────────────────────────────────┐│
│  │ Database Statistics                                  ││
│  │                                                       ││
│  │  [ Load Statistics ]                                 ││
│  │                                                       ││
│  │  (Statistics will appear here)                       ││
│  └─────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────┘
```

---

## First Import

### Quick Test Import

Let's import data for just one country to verify everything works:

```
Step 1: Test Database Connection
─────────────────────────────────
1. Click "Test Connection" button
2. You should see: "✅ Database connection successful!"
3. If error, verify .env configuration

Step 2: Start Import
────────────────────
1. Select Year: 2022
2. Enter Countries: US
3. Leave "Clear existing data" unchecked
4. Click "Create Database"

Step 3: Monitor Progress
────────────────────────
A progress panel will appear showing:
  Status: Processing
  Current Step: Importing US/imports/trade.csv
  Records Imported: 1,234
  Countries Processed: 1/1
  [████████████████░░░░] 80%

Step 4: Completion
─────────────────
When complete, you'll see:
  Status: Completed
  Current Step: Import completed. Total records: 15,420
  Records Imported: 15,420
  Countries Processed: 3/3
  [████████████████████] 100%
```

### Verify Import

```bash
# Connect to database
psql -U postgres -d exiobase

# Check record count
SELECT COUNT(*) FROM public.trade WHERE year = 2022 AND region1 = 'US';

# View sample data
SELECT * FROM public.trade LIMIT 5;

# Check statistics
SELECT
    region1,
    tradeflow_type,
    COUNT(*) as record_count
FROM public.trade
WHERE year = 2022
GROUP BY region1, tradeflow_type;
```

### Full Import (All Countries)

```
1. Select Year: 2022
2. Leave Countries blank (imports all)
3. Click "Create Database"

Expected Duration:
├─ Per Country: ~30 minutes
├─ 15 Countries: ~7.5 hours
└─ Total Records: ~200,000 - 500,000

Note: This is a long-running operation.
You can close the browser and check back later.
Use the status endpoint to monitor progress.
```

---

## Troubleshooting

### Common Issues and Solutions

#### Issue 1: "TRADE_DATA_REPO_PATH environment variable not set"

**Error Message:**
```
System.InvalidOperationException: TRADE_DATA_REPO_PATH environment variable not set
```

**Solution:**
```bash
# Verify .env file exists
ls .env

# Verify TRADE_DATA_REPO_PATH is set
cat .env | grep TRADE_DATA_REPO_PATH

# Verify path exists
ls ../trade-data/

# If missing, create .env from example
cp .env.example .env
nano .env  # Set TRADE_DATA_REPO_PATH=../trade-data
```

---

#### Issue 2: "Database connection failed"

**Error Message:**
```
Npgsql.PostgresException: Connection refused
```

**Solution:**
```bash
# Check PostgreSQL is running
sudo systemctl status postgresql  # Linux
# or
pg_ctl status  # Windows

# Check connection string
cat .env | grep DATABASE_

# Test connection manually
psql -h localhost -U postgres -d exiobase

# Check firewall (Azure)
# - Add your IP to firewall rules in Azure Portal
```

---

#### Issue 3: "No CSV files found"

**Error Message:**
```
No CSV files found for 2022/US/imports
```

**Solution:**
```bash
# Verify trade-data repository exists
ls ../trade-data/

# Check file structure
ls ../trade-data/year/2022/US/imports/

# Should see files like:
# trade.csv
# trade_employment.csv
# trade_factor.csv
# etc.

# If missing, clone trade-data repo
cd ..
git clone https://github.com/ModelEarth/trade-data.git
cd trade
```

---

#### Issue 4: Application won't start on port 5094

**Error Message:**
```
Failed to bind to address http://localhost:5094: address already in use
```

**Solution:**
```bash
# Option 1: Kill process using port
# Windows:
netstat -ano | findstr :5094
taskkill /PID <process_id> /F

# Linux/Mac:
lsof -i :5094
kill -9 <process_id>

# Option 2: Change port in launchSettings.json
# Edit ModelEarth/Properties/launchSettings.json
# Change "applicationUrl": "http://localhost:5095"
```

---

#### Issue 5: Migration scripts fail

**Error Message:**
```
ERROR:  relation "trade" already exists
```

**Solution:**
```bash
# Drop and recreate database
psql -U postgres
DROP DATABASE exiobase;
CREATE DATABASE exiobase;
\q

# Run migrations again
psql -U postgres -d exiobase -f ModelEarth/DB\ Scripts/Postgres/001_CreateTradeTable.sql
psql -U postgres -d exiobase -f ModelEarth/DB\ Scripts/Postgres/002_CreateAdditionalTables.sql
psql -U postgres -d exiobase -f ModelEarth/DB\ Scripts/Postgres/003_CreateStoredProcs.sql
```

---

## Running Tests

### Quick Test Run

```bash
# Navigate to test project
cd ModelEarth.Tests

# Run all tests
dotnet test

# Expected output:
# Passed!  - Failed:     0, Passed:    35, Skipped:     0, Total:    35
```

### Test Categories

```bash
# Run only unit tests
dotnet test --filter "Category!=Integration"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CsvImportServiceTests"
```

See [Testing Guide](../testing/TESTING_GUIDE.md) for detailed testing documentation.

---

## Development Workflow

### Typical Development Cycle

```
1. Make code changes
   ↓
2. Run tests
   dotnet test
   ↓
3. Build application
   dotnet build
   ↓
4. Run locally
   dotnet run
   ↓
5. Test in browser
   http://localhost:5094
   ↓
6. Commit changes
   git add .
   git commit -m "Description"
   git push
```

### Hot Reload (Development)

```bash
# Run with hot reload (auto-restart on code changes)
dotnet watch run

# Edit any .cs file
# Application automatically rebuilds and restarts
```

---

## Next Steps

Now that you have the application running:

1. **Explore the API**: See [API Documentation](../API_DOCUMENTATION.md)
2. **Understand the Architecture**: See [Architecture Guide](../ARCHITECTURE.md)
3. **Write Tests**: See [Testing Guide](../testing/TESTING_GUIDE.md)
4. **Learn the Code**: See [Code Structure](../CODE_STRUCTURE.md)

---

## Quick Reference

### Essential Commands

```bash
# Start application
cd ModelEarth && dotnet run

# Run tests
cd ModelEarth.Tests && dotnet test

# Build solution
dotnet build

# Restore packages
dotnet restore

# Database connection
psql -U postgres -d exiobase

# View logs
tail -f logs/application.log
```

### Important Files

```
.env                           - Environment configuration
ModelEarth/Program.cs          - Application entry point
ModelEarth/appsettings.json   - App configuration
ModelEarth/DB Scripts/         - Database migrations
```

### Support

- **Documentation**: See `/docs` folder
- **Issues**: https://github.com/modelearth/trade/issues
- **Trade Data**: https://github.com/ModelEarth/trade-data

---

Happy importing! 🚀
