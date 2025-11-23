# Trade Data Import System - Architecture

## Table of Contents
- [Overview](#overview)
- [System Architecture](#system-architecture)
- [Data Flow](#data-flow)
- [Component Architecture](#component-architecture)
- [Database Architecture](#database-architecture)
- [Dependency Injection Pattern](#dependency-injection-pattern)

## Overview

The Trade Data Import System is a .NET 8.0 web application designed to import large-scale trade data from CSV files into a PostgreSQL database. The system processes **405 CSV files** (15 countries × 3 tradeflows × 9 file types) and consolidates them into **9 PostgreSQL tables**.

### Key Statistics
- **Input**: 405 CSV files per year
- **Output**: 9 consolidated database tables
- **Countries**: 15 (US, IN, CN, etc.)
- **Tradeflows**: 3 (imports, exports, domestic)
- **File Types**: 9 (trade, employment, factor, impact, material, resource, + 3 BEA tables)
- **Performance**: ~30 minutes per country, ~7.5 hours for full year

---

## System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        USER INTERFACE                            │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Year Selector│  │Country Filter│  │Progress View │          │
│  │  (2019/2022) │  │  (Optional)  │  │  (Real-time) │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│                         [Create Database Button]                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTP POST
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    REST API CONTROLLER                           │
│                 (TradeImportController)                          │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Endpoints:                                                │  │
│  │  • POST /api/tradeimport/create-database                 │  │
│  │  • GET  /api/tradeimport/status/{jobId}                  │  │
│  │  • GET  /api/tradeimport/statistics/{year}               │  │
│  │  • GET  /api/tradeimport/test-connection                 │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                   │
│         ┌─────────────────┴──────────────────┐                  │
│         ▼                                     ▼                  │
│  ┌─────────────────┐               ┌──────────────────┐        │
│  │ICsvImportService│               │ITradeDataRepository│       │
│  │   (Interface)   │               │    (Interface)     │       │
│  └────────┬────────┘               └─────────┬────────┘        │
│           │                                   │                  │
└───────────┼───────────────────────────────────┼──────────────────┘
            │                                   │
            ▼                                   ▼
┌─────────────────────┐           ┌──────────────────────┐
│  CsvImportService   │           │ TradeDataRepository  │
│   (Concrete Class)  │           │   (Concrete Class)   │
│                     │           │                      │
│  • File Discovery   │           │  • Bulk Insert       │
│  • CSV Parsing      │           │  • Delete Operations │
│  • Validation       │           │  • Statistics        │
│  • Table Mapping    │           │  • Connection Test   │
└──────────┬──────────┘           └──────────┬───────────┘
           │                                  │
           ▼                                  ▼
    ┌────────────┐                    ┌─────────────┐
    │CSV Files   │                    │ PostgreSQL  │
    │(405 files) │                    │  Database   │
    │            │                    │  (9 tables) │
    └────────────┘                    └─────────────┘
```

---

## Data Flow

### Complete Import Flow Diagram

```
START: User clicks "Create Database"
  │
  ▼
┌────────────────────────────────────────────┐
│ 1. Validate Request                        │
│    • Check year (2019 or 2022)            │
│    • Validate country codes               │
│    • Generate unique Job ID               │
└────────────┬───────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ 2. Start Background Task                  │
│    • Create ImportProgress object         │
│    • Store in _importJobs dictionary      │
│    • Return Job ID to client              │
└────────────┬───────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ 3. Clear Existing Data (Optional)         │
│    IF clearExistingData == true:          │
│      • Call ClearYearDataAsync()          │
│      • Delete from all 9 tables           │
│      • WHERE year = selected_year         │
└────────────┬───────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ 4. Get Available Countries                │
│    • Scan trade-data/year/{year}/         │
│    • Filter by requested countries        │
│    • Return list of 2-char country codes  │
└────────────┬───────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ 5. FOR EACH Country                       │
│    ├─ FOR EACH Tradeflow (3 types)       │
│    │   ├─ imports/                        │
│    │   ├─ exports/                        │
│    │   └─ domestic/                       │
│    │                                       │
│    │   ▼                                  │
│    │  ┌─────────────────────────────────┐│
│    │  │ 5a. Discover CSV Files          ││
│    │  │  • GetCsvFilesForImport()       ││
│    │  │  • Path: year/2022/US/imports/  ││
│    │  │  • Returns ~9 CSV files         ││
│    │  └──────────────┬──────────────────┘│
│    │                 │                    │
│    │                 ▼                    │
│    │  ┌─────────────────────────────────┐│
│    │  │ 5b. FOR EACH CSV File           ││
│    │  │  ├─ trade.csv                   ││
│    │  │  ├─ trade_employment.csv        ││
│    │  │  ├─ trade_factor.csv            ││
│    │  │  ├─ trade_impact.csv            ││
│    │  │  ├─ trade_material.csv          ││
│    │  │  ├─ trade_resource.csv          ││
│    │  │  ├─ bea_table1.csv              ││
│    │  │  ├─ bea_table2.csv              ││
│    │  │  └─ bea_table3.csv              ││
│    │  │                                  ││
│    │  │  ▼                               ││
│    │  │ ┌──────────────────────────────┐││
│    │  │ │ 5b.1 Read & Parse CSV        │││
│    │  │ │  • ReadCsvFileAsync()        │││
│    │  │ │  • Using CsvHelper           │││
│    │  │ │  • Returns List<TradeImport> │││
│    │  │ └──────────┬───────────────────┘││
│    │  │            │                     ││
│    │  │            ▼                     ││
│    │  │ ┌──────────────────────────────┐││
│    │  │ │ 5b.2 Map to Trade Objects    │││
│    │  │ │  • Set Year                  │││
│    │  │ │  • Set Region1/Region2       │││
│    │  │ │  • Set TradeflowType         │││
│    │  │ │  • Set SourceFile            │││
│    │  │ └──────────┬───────────────────┘││
│    │  │            │                     ││
│    │  │            ▼                     ││
│    │  │ ┌──────────────────────────────┐││
│    │  │ │ 5b.3 Determine Target Table  │││
│    │  │ │  • GetTableNameFromFileName()│││
│    │  │ │  • trade.csv → public.trade  │││
│    │  │ │  • bea1.csv → public.bea_t1  │││
│    │  │ └──────────┬───────────────────┘││
│    │  │            │                     ││
│    │  │            ▼                     ││
│    │  │ ┌──────────────────────────────┐││
│    │  │ │ 5b.4 Bulk Insert to DB       │││
│    │  │ │  • BulkInsertAsync()         │││
│    │  │ │  • Batch size: 1000 records  │││
│    │  │ │  • Using Dapper              │││
│    │  │ │  • Parameterized SQL         │││
│    │  │ └──────────┬───────────────────┘││
│    │  │            │                     ││
│    │  │            ▼                     ││
│    │  │ ┌──────────────────────────────┐││
│    │  │ │ 5b.5 Update Progress         │││
│    │  │ │  • Increment recordsImported │││
│    │  │ │  • Update currentStep        │││
│    │  │ └──────────────────────────────┘││
│    │  └─────────────────────────────────┘│
│    └───────────────────────────────────────┘
└────────────┬───────────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ 6. Complete Import                         │
│    • Set status = "Completed"             │
│    • Set completedAt timestamp            │
│    • Log final statistics                 │
└────────────┬───────────────────────────────┘
             │
             ▼
          [END]
```

### Client Polling Flow

```
User Interface (Browser)
  │
  │ [Every 2 seconds]
  │
  ├─► GET /api/tradeimport/status/{jobId}
  │      │
  │      ├─► Returns: {
  │      │     "status": "Processing",
  │      │     "recordsImported": 15420,
  │      │     "countriesProcessed": 2,
  │      │     "currentStep": "Importing US/imports/trade.csv"
  │      │   }
  │      │
  │      └─► Update UI:
  │           • Progress bar
  │           • Record count
  │           • Current step text
  │
  └─► [Repeat until status = "Completed" or "Failed"]
```

---

## Component Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌────────────────┐  ┌────────────────┐                    │
│  │ Web UI (MVC)   │  │  REST API      │                    │
│  │ - Views/       │  │  - Controllers/│                    │
│  │   TradeImport/ │  │    TradeImport │                    │
│  └────────────────┘  └────────────────┘                    │
└────────────────────────┬────────────────────────────────────┘
                         │ Dependency Injection
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Service Interfaces (Contracts)                │  │
│  │  ┌──────────────────┐  ┌──────────────────────────┐ │  │
│  │  │ICsvImportService │  │ITradeDataRepository      │ │  │
│  │  │                  │  │                          │ │  │
│  │  │ + GetCsvFiles()  │  │ + BulkInsertAsync()     │ │  │
│  │  │ + ReadCsvAsync() │  │ + ClearYearDataAsync()  │ │  │
│  │  │ + ValidateFiles()│  │ + GetStatisticsAsync()  │ │  │
│  │  │ + GetCountries() │  │ + TestConnectionAsync() │ │  │
│  │  └──────────────────┘  └──────────────────────────┘ │  │
│  └──────────────┬───────────────────┬───────────────────┘  │
│                 │                   │                       │
│                 ▼                   ▼                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │       Service Implementations                         │  │
│  │  ┌──────────────────┐  ┌──────────────────────────┐ │  │
│  │  │ CsvImportService │  │ TradeDataRepository      │ │  │
│  │  │                  │  │                          │ │  │
│  │  │ Uses: CsvHelper  │  │ Uses: Dapper + Npgsql   │ │  │
│  │  │ Reads: FileSystem│  │ Connects: PostgreSQL    │ │  │
│  │  └──────────────────┘  └──────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      DATA LAYER                              │
│  ┌────────────────┐              ┌────────────────────────┐ │
│  │  CSV Files     │              │   PostgreSQL Database  │ │
│  │  (File System) │              │                        │ │
│  │                │              │  • public.trade        │ │
│  │  trade-data/   │              │  • public.trade_empl.. │ │
│  │  └─year/       │              │  • public.trade_factor │ │
│  │    └─2022/     │              │  • public.trade_impact │ │
│  │      └─US/     │              │  • public.trade_mat... │ │
│  │        ├imports│              │  • public.trade_res... │ │
│  │        ├exports│              │  • public.bea_table1   │ │
│  │        └domestic              │  • public.bea_table2   │ │
│  │                │              │  • public.bea_table3   │ │
│  └────────────────┘              └────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## Database Architecture

### Table Consolidation Pattern

```
CSV Files (405 total)                    Database Tables (9 total)
─────────────────────                    ──────────────────────────

year/2022/US/imports/trade.csv ─┐
year/2022/US/exports/trade.csv ─┼──►  public.trade
year/2022/US/domestic/trade.csv─┘       ├─ year (SMALLINT)
                                         ├─ region1 (CHAR(2))
year/2022/IN/imports/trade.csv ─┐       ├─ region2 (CHAR(2))
year/2022/IN/exports/trade.csv ─┼──►    ├─ tradeflow_type (VARCHAR)
year/2022/IN/domestic/trade.csv─┘       └─ amount (NUMERIC)
                                         [Discriminator columns]
... (15 countries × 3 flows) ...

─────────────────────────────────────────────────────────────

year/2022/US/imports/trade_employment.csv ─┐
year/2022/US/exports/trade_employment.csv ─┼──► public.trade_employment
year/2022/US/domestic/trade_employment.csv─┘

... (Similar pattern for all 9 table types) ...

─────────────────────────────────────────────────────────────

year/2022/US/imports/bea_table1.csv ─┐
year/2022/US/exports/bea_table1.csv ─┼──► public.bea_table1
year/2022/US/domestic/bea_table1.csv─┘
```

### Database Schema

```sql
-- Example: public.trade table schema
CREATE TABLE public.trade (
    trade_id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,
    region1 CHAR(2) NOT NULL,      -- Source country
    region2 CHAR(2) NOT NULL,      -- Destination country
    industry1 TEXT NOT NULL,        -- Source industry
    industry2 TEXT NOT NULL,        -- Destination industry
    amount NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20) NOT NULL,  -- 'imports', 'exports', 'domestic'
    source_file VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_trade_year ON public.trade(year);
CREATE INDEX idx_trade_regions ON public.trade(region1, region2);
CREATE INDEX idx_trade_tradeflow ON public.trade(tradeflow_type);
CREATE INDEX idx_trade_year_region1 ON public.trade(year, region1);
```

---

## Dependency Injection Pattern

### Interface-Based DI Architecture

```
┌──────────────────────────────────────────────────────┐
│          Startup (Program.cs)                        │
│                                                       │
│  builder.Services.AddScoped<                        │
│    ICsvImportService,      ──────────┐              │
│    CsvImportService>();              │              │
│                                       │              │
│  builder.Services.AddScoped<         │              │
│    ITradeDataRepository,   ──────────┼──┐          │
│    TradeDataRepository>();           │  │          │
│                                       │  │          │
└───────────────────────────────────────┼──┼──────────┘
                                        │  │
                    Registers interfaces│  │with implementations
                                        │  │
                                        ▼  ▼
┌──────────────────────────────────────────────────────┐
│       DI Container (Built-in .NET)                   │
│                                                       │
│  ┌─────────────────┐      ┌──────────────────────┐ │
│  │ICsvImportService│      │ITradeDataRepository  │ │
│  │      ═══▶       │      │       ═══▶          │ │
│  │CsvImportService │      │TradeDataRepository  │ │
│  └─────────────────┘      └──────────────────────┘ │
│                                                       │
└───────────────────────┬───────────────────────────────┘
                        │
                  Injects into
                        │
                        ▼
┌──────────────────────────────────────────────────────┐
│       TradeImportController                          │
│                                                       │
│  public TradeImportController(                      │
│      ICsvImportService csvService,    ◄────┐        │
│      ITradeDataRepository repository, ◄────┼──┐    │
│      ILogger<TradeImportController> logger) │  │    │
│  {                                          │  │    │
│      _csvService = csvService;    ─────────┘  │    │
│      _repository = repository;    ────────────┘    │
│  }                                                  │
│                                                       │
└──────────────────────────────────────────────────────┘
```

### Benefits of This Pattern

1. **Testability**: Can mock `ICsvImportService` and `ITradeDataRepository` in tests
2. **Loose Coupling**: Controller depends on abstractions, not concrete implementations
3. **Flexibility**: Easy to swap implementations without changing controller code
4. **SOLID Principles**: Follows Dependency Inversion Principle
5. **Clean Architecture**: Clear separation of concerns

### Testing with DI

```csharp
// In Tests: Mock the interfaces
var mockCsvService = new Mock<ICsvImportService>();
var mockRepository = new Mock<ITradeDataRepository>();

// Setup behavior
mockCsvService.Setup(x => x.GetCsvFilesForImport(2022, "US", "imports"))
              .Returns(new List<string> { "trade.csv", "trade_employment.csv" });

// Inject mocks into controller
var controller = new TradeImportController(
    mockCsvService.Object,
    mockRepository.Object,
    mockLogger.Object
);

// Test controller behavior in isolation
```

---

## Technology Stack

```
┌─────────────────────────────────────────────────────┐
│                    Frontend                          │
│  • Bootstrap 5 (UI Components)                      │
│  • JavaScript (AJAX polling)                        │
│  • Razor Views (.cshtml)                            │
└─────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────┐
│                   Backend                            │
│  • .NET 8.0 (ASP.NET Core MVC)                      │
│  • C# 12                                             │
│  • Dependency Injection (Built-in)                  │
└─────────────────────────────────────────────────────┘
                         │
              ┌──────────┴──────────┐
              ▼                     ▼
┌────────────────────┐  ┌──────────────────────┐
│   Data Access      │  │   File Processing    │
│  • Dapper (ORM)    │  │  • CsvHelper         │
│  • Npgsql (Driver) │  │  • dotenv.net        │
└────────────────────┘  └──────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│                   Database                           │
│  • PostgreSQL 12+                                    │
│  • Azure PostgreSQL (Production)                     │
│  • Local PostgreSQL (Development)                    │
└─────────────────────────────────────────────────────┘
```

---

## Performance Considerations

### Batch Processing Strategy

```
Single CSV File Import Flow:
─────────────────────────────

Read CSV (10,000 records)
         │
         ▼
┌─────────────────┐
│ Batch 1         │  ──► INSERT 1,000 records ─┐
│ (records 1-1000)│                             │
└─────────────────┘                             │
┌─────────────────┐                             │
│ Batch 2         │  ──► INSERT 1,000 records ─┤
│ (records 1001-  │                             │
│         2000)   │                             ├─► To Database
└─────────────────┘                             │   (Minimizes
         ...                                     │    connection
┌─────────────────┐                             │    overhead)
│ Batch 10        │  ──► INSERT 1,000 records ─┘
│ (records 9001-  │
│        10000)   │
└─────────────────┘

Total: 10 batch inserts instead of 10,000 individual inserts
Performance gain: ~100x faster
```

### Optimization Strategies

1. **Batch Inserts**: 1,000 records per batch (configurable via `BATCH_SIZE` env var)
2. **Async Operations**: All I/O operations use `async/await`
3. **Background Jobs**: Import runs in background thread via `Task.Run()`
4. **Database Indexes**: Strategic indexes on year, regions, tradeflow_type
5. **Connection Pooling**: Npgsql connection pool (default: 100 connections)

---

## Next Steps

- [Testing Guide](./testing/TESTING_GUIDE.md) - Comprehensive testing documentation
- [Code Structure](./CODE_STRUCTURE.md) - Detailed code organization
- [Getting Started](./guides/GETTING_STARTED.md) - Quick start guide
- [API Documentation](./API_DOCUMENTATION.md) - REST API reference
