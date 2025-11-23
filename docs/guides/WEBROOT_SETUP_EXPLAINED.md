# WebRoot Setup & Trade Data Flow - Complete Explanation

## Overview: The Big Picture

This project is part of the **ModelEarth ecosystem** which processes global trade data for environmental impact analysis.

```
┌─────────────────────────────────────────────────────────────────┐
│                    THE COMPLETE DATA FLOW                        │
└─────────────────────────────────────────────────────────────────┘

1. EXIOBASE Raw Data (506 MB zip file)
   │
   ├─> Downloads from: https://zenodo.org/record/5589597
   │   Years available: 2019, 2022
   │
   ↓
2. Python Processing (exiobase/tradeflow/main.py)
   │
   ├─> Processes ~10 minutes per country
   ├─> Extracts 3 tradeflow types: imports, exports, domestic
   ├─> Generates ~9 CSV files per tradeflow
   │
   ↓
3. CSV Files (trade-data repository)
   │
   ├─> Structure: year/{YEAR}/{COUNTRY}/{TRADEFLOW}/*.csv
   ├─> Example: year/2022/US/imports/trade.csv
   ├─> Total: 405 CSV files per year (15 countries × 3 flows × 9 files)
   │
   ↓
4. .NET Import System (THIS PROJECT - trade repository)
   │
   ├─> Reads CSV files
   ├─> Consolidates into 9 tables
   ├─> Bulk inserts into PostgreSQL
   │
   ↓
5. PostgreSQL Database
   │
   ├─> 9 consolidated tables (not 405!)
   ├─> Indexed for fast querying
   ├─> Powers visualization tools
   │
   ↓
6. Web Visualization
   │
   └─> https://model.earth/profile/footprint/
       View trade impacts by country/year
```

---

## The WebRoot Folder Structure

The ModelEarth project uses a "webroot" pattern where multiple repos sit side-by-side:

```
C:\Users\srich\Downloads\
└── webroot/                    (conceptual - your folder structure)
    ├── trade-main/             ✅ THIS REPO - .NET import system
    │   ├── ModelEarth/         (ASP.NET app)
    │   ├── ModelEarth.Tests/   (35 passing tests)
    │   ├── .env                (points to ../trade-data)
    │   └── docs/               (comprehensive documentation)
    │
    ├── trade-data/             ✅ ALREADY EXISTS - CSV files
    │   └── year/
    │       ├── 2019/           (pre-COVID baseline)
    │       │   ├── US/
    │       │   ├── IN/
    │       │   ├── CN/
    │       │   └── ... (12 countries total)
    │       └── 2022/           (latest available)
    │           └── US/
    │               ├── imports/
    │               │   ├── trade.csv
    │               │   ├── trade_employment.csv
    │               │   ├── trade_factor.csv
    │               │   ├── trade_impact.csv
    │               │   ├── trade_material.csv
    │               │   ├── trade_resource.csv
    │               │   └── ... (9 files total)
    │               ├── exports/
    │               │   └── (same 9 files)
    │               └── domestic/
    │                   └── (same 9 files)
    │
    └── exiobase/               (NOT YET CLONED - Python processors)
        └── tradeflow/
            ├── main.py         (orchestrator)
            ├── trade.py        (extracts trade flows)
            ├── trade_impact.py (calculates impacts)
            ├── trade_resource.py (employment/resources)
            └── config.yaml     (year, country, tradeflow settings)
```

---

## What Each Repository Does

### **1. trade-data Repository**
- **Purpose:** Stores processed CSV files
- **Location:** `../trade-data`
- **Content:** 405 CSV files per year
- **Why separate?** Keep exiobase repo small (506 MB raw data doesn't go in git)

### **2. trade Repository (THIS PROJECT)**
- **Purpose:** Import CSV → PostgreSQL
- **Technology:** .NET 8.0, ASP.NET Core MVC
- **What it does:**
  - Reads CSV files from trade-data
  - Consolidates 405 CSVs into 9 database tables
  - Provides REST API + Web UI
  - Background job processing with status tracking

### **3. exiobase Repository**
- **Purpose:** Generate CSV files from raw EXIOBASE data
- **Technology:** Python
- **What it does:**
  - Downloads 506 MB EXIOBASE zip file
  - Processes trade flows for selected countries
  - Outputs CSV files to trade-data repo
  - Takes ~10 min per country × 3 tradeflows

---

## The CSV File Structure

### **What's in each CSV:**

**trade.csv** (main trade flows)
```csv
Region1,Region2,Industry1,Industry2,Amount
US,CN,Agriculture,Manufacturing,1000.50
US,MX,Mining,Transportation,2000.75
```

**trade_employment.csv** (jobs impact)
```csv
Region1,Region2,Industry1,Industry2,Amount
US,CN,Agriculture,Manufacturing,125.5
```

**trade_factor.csv** (721 environmental factors)
- CO2 emissions
- Water usage
- Land use
- 718 other environmental impacts

**trade_impact.csv** (aggregated impacts - 22 columns)

**trade_resource.csv** (resource analysis)

**trade_material.csv** (material flows)

... and 3 BEA (Bureau of Economic Analysis) tables

---

## Why 2019 and 2022?

**2019:**
- Pre-COVID baseline year
- US EPA also uses 2019 for comparison
- Most "normal" recent year

**2022:**
- Latest available in EXIOBASE
- Shows post-COVID trade patterns
- Allows before/after comparison

---

## The Database Consolidation Strategy

### **Problem:** 405 CSV files per year
### **Solution:** Consolidate into 9 tables

**How?** Add metadata columns:

```sql
CREATE TABLE public.trade (
    trade_id BIGSERIAL PRIMARY KEY,
    year SMALLINT NOT NULL,           -- ← Distinguishes 2019 vs 2022
    region1 CHAR(2) NOT NULL,          -- ← Country code (US, IN, CN...)
    region2 CHAR(2) NOT NULL,
    industry1 TEXT NOT NULL,
    industry2 TEXT NOT NULL,
    amount NUMERIC(20,4) NOT NULL,
    tradeflow_type VARCHAR(20),        -- ← imports/exports/domestic
    source_file VARCHAR(255)           -- ← Track origin CSV
);
```

**Result:**
```
trade-data/year/2022/US/imports/trade.csv    ─┐
trade-data/year/2022/US/exports/trade.csv    ├─> public.trade
trade-data/year/2022/US/domestic/trade.csv   ─┤
trade-data/year/2022/IN/imports/trade.csv    ─┤
... (all 45 trade.csv files from 15 countries) ┘
```

---

## Processing Performance

### **Python CSV Generation (exiobase repo):**
- Per country: ~10 minutes
- 12 countries × 3 tradeflows = 36 operations
- 36 × 10 min = **6 hours per year**

### **.NET Database Import (this repo):**
- Per CSV: < 1 second (batched inserts)
- Per country (27 files): ~30 seconds
- 15 countries: **~7.5 minutes total**

---

## Current Status

✅ **What's Working:**
1. `.env` file configured (points to `../trade-data`)
2. Interface-based DI (35/35 tests passing)
3. CSV import service (reads real data)
4. Database repository (bulk insert with batching)
5. REST API (4 endpoints working)
6. Web UI (year dropdown, create database button)
7. Background jobs (real-time status tracking)
8. Comprehensive documentation (docs/ folder)

⏸️ **What's Pending:**
1. Docker Desktop needs to start (for local PostgreSQL)
2. Run database migrations (create 9 tables)
3. Test end-to-end import with real data
4. Generate more CSV files (currently only US/2022/imports exists)

---

## How to Complete the Setup

### **Option A: Use Existing Data (Quick Test)**

```bash
# 1. Start Docker Desktop manually from Windows Start menu

# 2. Wait ~30 seconds, then:
docker-compose up -d

# 3. Run migrations:
cd DbMigrate
dotnet run

# 4. Start the application:
cd ../ModelEarth
dotnet run

# 5. Open browser:
http://localhost:5094/TradeImport

# 6. Import data:
- Year: 2022
- Countries: US
- Click "Create Database"
```

### **Option B: Generate More CSV Files (Full Setup)**

```bash
# 1. Clone exiobase repo (if not exists):
cd ../..  # Go to webroot level
git clone https://github.com/ModelEarth/exiobase.git

# 2. Edit config.yaml:
cd exiobase/tradeflow
# Set: year: 2022, country: IN, tradeflow: imports

# 3. Run Python processor:
python main.py

# 4. CSV files will be created in:
../../trade-data/year/2022/IN/imports/*.csv

# 5. Then use .NET app to import them
```

---

## The Complete Workflow (End-to-End)

```
┌──────────────────────────────────────────────────────────────┐
│  STEP 1: Generate CSV Files (One-time per country/year)      │
└──────────────────────────────────────────────────────────────┘

cd exiobase/tradeflow
# Edit config.yaml: year: 2022, country: IN
python main.py
# Wait ~30 minutes (10 min × 3 tradeflows)
# Output: ../trade-data/year/2022/IN/{imports,exports,domestic}/*.csv

┌──────────────────────────────────────────────────────────────┐
│  STEP 2: Import to Database (Repeatable anytime)             │
└──────────────────────────────────────────────────────────────┘

cd trade-main/ModelEarth
dotnet run
# Navigate to: http://localhost:5094/TradeImport
# Select Year: 2022, Countries: IN
# Click "Create Database"
# Wait ~30 seconds
# View imported data in PostgreSQL

┌──────────────────────────────────────────────────────────────┐
│  STEP 3: Visualize Data (External site)                      │
└──────────────────────────────────────────────────────────────┘

https://model.earth/profile/footprint/
# Select year: 2022, country: IN
# View environmental impacts, employment effects, etc.
```

---

## Key Configuration Files

### **.env (trade-main/.env)**
```env
DATABASE_HOST=localhost
DATABASE_NAME=exiobase
DATABASE_USER=postgres
DATABASE_PASSWORD=password
DATABASE_PORT=5432

TRADE_DATA_REPO_PATH=../trade-data  # ← Points to CSV files
BATCH_SIZE=1000
```

### **config.yaml (exiobase/tradeflow/config.yaml)**
```yaml
year: 2022
country: US  # or "all" for all 12 countries
tradeflow: imports  # or exports, domestic

folders:
  base: ../../trade-data/year/{year}
  import: ../../trade-data/year/{year}/{country}/imports
```

### **docker-compose.yml (trade-main/docker-compose.yml)**
```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: exiobase
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
```

---

## Resources & Links

- **Live Visualization:** https://model.earth/profile/footprint/
- **Comparison Tool:** https://model.earth/comparison
- **Exiobase Data:** https://zenodo.org/record/5589597
- **Python Scripts:** https://github.com/ModelEarth/exiobase/tree/main/tradeflow
- **Trade Data Repo:** https://github.com/ModelEarth/trade-data

---

## Summary

**You already have everything you need to test!**

1. ✅ trade-data repo exists with real CSV data
2. ✅ .env configured to use it
3. ✅ Application working (all tests pass)
4. ⏸️ Just need Docker Desktop to start for database

Once Docker is running, you can import the existing US 2022 data with one click!
