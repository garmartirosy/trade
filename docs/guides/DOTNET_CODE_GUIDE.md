# .NET Code Guide - Trade Data Import System

Complete guide to understanding the .NET 8.0 codebase for the CSV-to-SQL trade data import system.

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Core Components](#core-components)
- [Data Flow](#data-flow)
- [Database Layer](#database-layer)
- [API Endpoints](#api-endpoints)
- [Testing Strategy](#testing-strategy)
- [Configuration Management](#configuration-management)

## Architecture Overview

### Design Pattern
The system uses **Interface-Based Dependency Injection** with the following layers:

```
Presentation Layer (Controllers/Views)
        ↓
Service Layer (ICsvImportService)
        ↓
Repository Layer (ITradeDataRepository)
        ↓
Database (PostgreSQL)
```

### Key Architectural Decisions

1. **Interface Segregation**: All services implement interfaces for testability
2. **Background Processing**: Long-running imports don't block HTTP requests
3. **Batch Processing**: Database inserts use configurable batch sizes (default: 1000)
4. **Stateless Services**: Services maintain no internal state between requests

## Project Structure

```
ModelEarth/                          # Main ASP.NET Core MVC application
├── Controllers/
│   └── TradeImportController.cs     # REST API for import operations
├── Models/Data/
│   ├── Trade.cs                     # Domain model for trade data
│   ├── TradeImportRecord.cs         # CSV-to-domain mapping model
│   ├── ImportStatus.cs              # Background job status tracking
│   └── DatabaseCreationRequest.cs   # API request model
├── Services/
│   ├── ICsvImportService.cs         # CSV operations interface
│   ├── CsvImportService.cs          # CSV file discovery & parsing
│   ├── ITradeDataRepository.cs      # Data access interface
│   └── TradeDataRepository.cs       # PostgreSQL operations with Dapper
├── Views/TradeImport/
│   └── Index.cshtml                 # Web UI for import operations
├── DB Scripts/Postgres/
│   ├── 001_CreateTradeTable.sql     # Migration: main trade table
│   ├── 002_CreateAdditionalTables.sql  # Migration: 8 additional tables
│   └── 003_CreateStoredProcs.sql    # Stored procedures (future use)
└── Program.cs                       # Application bootstrap & DI setup

ModelEarth.Tests/                    # xUnit test project
├── Controllers/
│   └── TradeImportControllerTests.cs   # API endpoint tests (12 tests)
├── Services/
│   └── CsvImportServiceTests.cs     # CSV parsing tests (18 tests)
└── Integration/
    └── CsvImportIntegrationTests.cs # End-to-end tests (5 tests)

DbMigrate/                           # Database migration console app
└── Program.cs                       # Executes migration scripts
```

## Core Components

### 1. CsvImportService

**Purpose**: Discovers and parses CSV files from the trade-data repository

**Key Methods**:
- `GetCsvFilesForImport(int year, string countryCode, string tradeflow)` - Discovers CSV files
- `ReadCsvFile(string filePath)` - Parses CSV into records
- `ConvertToTrade(List<TradeImportRecord> records)` - Maps to domain models
- Similar conversion methods for 8 other table types

**CSV File Mapping**:
- `trade.csv` → `public.trade`
- `trade_employment.csv` → `public.trade_employment`
- `trade_factor.csv` → `public.trade_factor`
- `trade_impact.csv` → `public.trade_impact`
- `trade_material.csv` → `public.trade_material`
- `trade_resource.csv` → `public.trade_resource`
- `bea_table1.csv` → `public.bea_table1`
- `bea_table2.csv` → `public.bea_table2`
- `bea_table3.csv` → `public.bea_table3`

### 2. TradeDataRepository

**Purpose**: Handles all PostgreSQL database operations using Dapper

**Key Methods**:
- `InsertTradeDataAsync(List<Trade> trades)` - Bulk inserts with batching
- `GetDataStatisticsAsync(int year)` - Returns record counts per table
- `TestConnectionAsync()` - Verifies database connectivity
- `ClearExistingDataAsync()` - Clears all trade data for a year

**Batch Processing Strategy**:
1. Default batch size: 1000 records (configurable via `BATCH_SIZE`)
2. Transaction per batch: Each batch commits independently
3. Error handling: Failed batches roll back without affecting previous batches
4. Logging: Progress logged after each batch

### 3. TradeImportController

**Purpose**: REST API for import operations with background job management

**Key Endpoints**:
- `POST /api/tradeimport/create-database` - Start import job
- `GET /api/tradeimport/status/{jobId}` - Poll job status
- `GET /api/tradeimport/statistics/{year}` - Get data statistics
- `GET /api/tradeimport/test-connection` - Test database connection

**Background Processing**: Uses `Task.Run()` for non-blocking imports with in-memory job tracking

## Data Flow

### End-to-End Import Process

```
1. User Request
   ↓ POST /api/tradeimport/create-database
   ↓ { year: 2022, countries: ["US"], clearExistingData: false }
   ↓
2. Controller Creates Job
   ↓ jobId: "abc-123"
   ↓ Task.Run() → Background Processing
   ↓ Returns: { jobId: "abc-123", message: "Import started" }
   ↓
3. CSV Discovery
   ↓ GetCsvFilesForImport(2022, "US", "imports")
   ↓ Returns 9 CSV file paths
   ↓
4. CSV Parsing
   ↓ ReadCsvFile("trade.csv")
   ↓ Returns List<TradeImportRecord>
   ↓
5. Data Conversion
   ↓ ConvertToTrade(records)
   ↓ Returns List<Trade>
   ↓
6. Database Insert
   ↓ InsertTradeDataAsync(trades)
   ↓ Batch 1: INSERT 1000 → COMMIT
   ↓ Batch 2: INSERT 1000 → COMMIT
   ↓ ...
   ↓
7. Status Updates
   ↓ User polls: GET /api/tradeimport/status/abc-123
   ↓ Returns: { status: "running", processedRecords: 2000 }
   ↓
8. Completion
   ↓ status.Status = "completed"
```

## Database Layer

### Schema Design

**Table: `public.trade`** (Main trade flow data)
- Columns: id, year, country_code, tradeflow, io_code, commodity_code, value, created_at, updated_at
- Indexes: year + country_code, tradeflow
- Triggers: Auto-update updated_at column

Similar structure for 8 additional tables: `trade_employment`, `trade_factor`, `trade_impact`, `trade_material`, `trade_resource`, `bea_table1`, `bea_table2`, `bea_table3`

### Migration Strategy

**DbMigrate Console Application** executes SQL scripts in order:
1. `001_CreateTradeTable.sql` - Creates main trade table
2. `002_CreateAdditionalTables.sql` - Creates 8 additional tables
3. `003_CreateStoredProcs.sql` - Creates stored procedures (future use)

## API Endpoints

### POST /api/tradeimport/create-database

**Request**:
```json
{
  "year": 2022,
  "countries": ["US", "CN"],  // null for all 15 countries
  "clearExistingData": false
}
```

**Response**:
```json
{
  "jobId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "message": "Import started"
}
```

### GET /api/tradeimport/status/{jobId}

**Response**:
```json
{
  "jobId": "f47ac10b...",
  "status": "running",  // "running" | "completed" | "failed"
  "currentStep": "Processing US - imports",
  "processedRecords": 15000,
  "startTime": "2025-01-15T10:30:00Z",
  "endTime": null,
  "errorMessage": null
}
```

### GET /api/tradeimport/statistics/{year}

**Response**:
```json
{
  "trade": 45000,
  "trade_employment": 42000,
  "trade_factor": 40000,
  ...
}
```

### GET /api/tradeimport/test-connection

**Response**:
```json
{
  "connected": true
}
```

## Testing Strategy

### Test Pyramid

- **5 Integration Tests**: End-to-end workflows with real CSV files
- **12 Controller Tests**: API endpoint behavior and validation
- **18 Unit Tests**: CSV discovery, parsing, and conversion logic

### Running Tests

```bash
# Run all tests
cd ModelEarth.Tests
dotnet test

# Run specific test suite
dotnet test --filter "FullyQualifiedName~CsvImportServiceTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Configuration Management

### Environment Variables (.env)

```env
# Database Configuration
DATABASE_HOST=localhost
DATABASE_NAME=exiobase
DATABASE_USER=postgres
DATABASE_PASSWORD=your_password
DATABASE_PORT=5432

# Trade Data Repository Path
TRADE_DATA_REPO_PATH=../trade-data

# Import Configuration
BATCH_SIZE=1000
```

### Docker Compose

```bash
# Start PostgreSQL
docker-compose up -d

# Stop PostgreSQL
docker-compose down
```

## Dependency Injection Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<ICsvImportService, CsvImportService>();
builder.Services.AddSingleton<ITradeDataRepository, TradeDataRepository>();

// Service lifetimes:
// - Singleton: Stateless services (CsvImportService, TradeDataRepository)
// - Scoped: Per-request state (not used)
// - Transient: Created per injection (not used)
```

## Performance Considerations

### Batch Insert Optimization

- **Before**: Individual INSERTs = 4 million network roundtrips = 10 hours/country
- **After**: Batch processing (1000 records/transaction) = 30 minutes/country (20x faster)

### Indexing Strategy

Indexes on year + country_code and tradeflow for optimized lookups

### Memory Management

CSV files streamed line-by-line (constant memory usage)

## Error Handling

- **Controller Level**: Validates requests, returns appropriate HTTP status codes
- **Background Processing**: Captures exceptions in job status (doesn't throw)
- **Repository Level**: Transaction rollback on batch failure, logs error, throws to caller

## Logging

Uses structured logging with placeholders:
```csharp
_logger.LogInformation("Processing {Country} - {Tradeflow}", country, tradeflow);
```

**Log Levels**:
- Info: General flow progress
- Warning: Recoverable issues (e.g., missing CSV files)
- Error: Failures (e.g., database insert errors)

## Production Readiness

### Current Implementation (MVP)
- In-memory job storage (use Redis/SQL for distributed systems)
- No authentication (add `[Authorize]` attributes)
- No rate limiting (add rate limiter middleware)

### Next Steps
1. Replace in-memory job storage with distributed cache
2. Add authentication & authorization
3. Implement rate limiting
4. Add health checks (`/health` endpoint)

## Common Issues & Solutions

### "TRADE_DATA_REPO_PATH environment variable not set"
Ensure `.env` file exists with `TRADE_DATA_REPO_PATH=../trade-data`

### Database connection timeout
Verify PostgreSQL is running: `docker-compose ps`

### "Sequence contains no matching element" in tests
Add graceful skip when test CSV files not present

## Technologies

- .NET 8.0 (ASP.NET Core MVC)
- PostgreSQL (Npgsql driver)
- Dapper (micro-ORM)
- CsvHelper (CSV parsing)
- dotenv.net (environment variables)
- xUnit + Moq + FluentAssertions (testing)
- Bootstrap 5 (UI)

## Performance Metrics

- Import speed: ~30 minutes per country (3 tradeflows)
- Batch size: 1000 records/transaction
- Full dataset: 405 CSV files → 9 tables (15 countries × 3 tradeflows × 9 file types)
- Test coverage: 35 tests (100% passing)
