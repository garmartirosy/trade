# Complete Test Flow - From Zero to Working Database

## Current Status Check

**What you have NOW:**
```
../trade-data/year/2022/US/imports/trade.csv  (2 rows of sample data)
```

**What you DON'T have yet:**
- ❌ No other CSV files (trade_employment, trade_factor, etc.)
- ❌ No exports/ or domestic/ folders
- ❌ No other countries (IN, CN, etc.)
- ❌ No 2019 data

---

## Understanding: Migrations vs Data Population

```
┌──────────────────────────────────────────────────────────────┐
│  STEP 1: MIGRATIONS (creates empty structure)                │
└──────────────────────────────────────────────────────────────┘

What: SQL scripts create 9 empty tables
Tool: DbMigrate/Program.cs
Input: 001_CreateTradeTable.sql, 002_CreateAdditionalTables.sql
Output: Empty PostgreSQL tables ready to receive data

Result after migrations:
  public.trade             → 0 rows (empty table)
  public.trade_employment  → 0 rows (empty table)
  public.trade_factor      → 0 rows (empty table)
  ... 6 more empty tables


┌──────────────────────────────────────────────────────────────┐
│  STEP 2: DATA POPULATION (fills tables with CSV data)        │
└──────────────────────────────────────────────────────────────┘

What: .NET app reads CSVs and inserts rows into tables
Tool: IndustryDB web application
Input: CSV files from trade-data/year/{year}/{country}/{tradeflow}/*.csv
Output: Populated database tables

Result after import:
  public.trade             → 2 rows (from US imports/trade.csv)
  public.trade_employment  → 0 rows (no CSV exists yet)
  public.trade_factor      → 0 rows (no CSV exists yet)
  ... etc.
```

---

## Three Different Test Scenarios

### **Scenario A: Quick Test (Use Existing Sample Data)** ⚡
**Time:** ~2 minutes
**Data:** 2 rows in 1 table
**Goal:** Verify system works end-to-end

```bash
# Prerequisites: Docker Desktop running

# 1. Start PostgreSQL
docker-compose up -d

# 2. Create empty tables
cd DbMigrate
dotnet run

# 3. Start web app
cd ../IndustryDB
dotnet run

# 4. Open browser → http://localhost:5094/TradeImport
#    Year: 2022
#    Countries: US
#    Click "Create Database"

# 5. Verify import
# Expected result: 2 rows imported into public.trade
```

**What this proves:**
- ✅ Database connection works
- ✅ CSV reading works
- ✅ Bulk insert works
- ✅ Status tracking works
- ⚠️ But only 1 file imported (trade.csv), 8 tables remain empty

---

### **Scenario B: Medium Test (Generate Full US Data with Python)** 🐍
**Time:** ~30 minutes (10 min × 3 tradeflows)
**Data:** ~5,000-10,000 rows across 9 tables
**Goal:** Test with realistic data volume

```bash
# 1. Clone exiobase repo (if not exists)
cd ../..  # Go to parent folder
git clone https://github.com/IndustryDB/exiobase.git
cd exiobase/tradeflow

# 2. Install Python dependencies
pip install pandas numpy pyyaml

# 3. Edit config.yaml:
year: 2022
country: US
tradeflow: imports  # Start with imports only

# 4. Run Python processor (first tradeflow)
python main.py
# Wait ~10 minutes
# Output: ../../trade-data/year/2022/US/imports/*.csv (9 files)

# 5. Repeat for other tradeflows:
# Edit config.yaml: tradeflow: exports
python main.py  # Wait ~10 min

# Edit config.yaml: tradeflow: domestic
python main.py  # Wait ~10 min

# 6. Verify CSV files created:
ls ../../trade-data/year/2022/US/imports/
# Should see: trade.csv, trade_employment.csv, trade_factor.csv, etc.

# 7. Now import with .NET app:
cd ../../trade-main/IndustryDB
dotnet run
# Browser → http://localhost:5094/TradeImport
# Year: 2022, Countries: US
# Click "Create Database"

# 8. Wait ~30 seconds for import
# Expected: All 9 tables populated with real data
```

**What this proves:**
- ✅ Full end-to-end workflow
- ✅ All 9 CSV file types generated
- ✅ All 3 tradeflow types (imports/exports/domestic)
- ✅ Realistic data volumes
- ✅ BEA integration (if BEA files exist)

---

### **Scenario C: Full Test (12 Countries, Multiple Years)** 🌍
**Time:** ~6 hours per year (Python) + ~10 minutes (import)
**Data:** ~200,000-500,000 rows
**Goal:** Production-ready full dataset

```bash
# 1. Edit config.yaml:
year: 2022
country: all  # Process all 12 countries
tradeflow: imports

# 2. Run Python (this will take ~2 hours for imports only)
python main.py

# 3. Repeat for exports and domestic
# Total: 12 countries × 3 tradeflows × 10 min = ~6 hours

# 4. Import with .NET app:
# Year: 2022, Countries: (leave blank = all)
# Expected: 200,000+ rows across all tables

# 5. Repeat for 2019 if needed
```

---

## Recommended Test Flow for YOU

**I recommend Scenario A first**, then B if you want more data:

```
┌────────────────────────────────────────────────────────────┐
│  RECOMMENDED: Scenario A (Quick Verification)              │
└────────────────────────────────────────────────────────────┘

✅ You already have CSV file: ../trade-data/year/2022/US/imports/trade.csv
✅ No Python needed (use existing sample)
✅ Takes 2 minutes total
✅ Proves the .NET import system works

Then if you want realistic data:
→ Run Scenario B to generate full US 2022 data with Python
```

---

## Step-by-Step: Scenario A (Quick Test)

### **1. Start Docker & PostgreSQL**
```bash
# Check Docker is running
docker ps

# If not running, start Docker Desktop from Windows Start menu
# Wait ~30 seconds, then:

# Start PostgreSQL container
docker-compose up -d

# Verify it's running
docker-compose ps
# Should show: trade-postgres (healthy)
```

### **2. Run Database Migrations**
```bash
cd DbMigrate
dotnet run
```

**Expected output:**
```
🔧 Running database migrations...
✅ Connected to database: exiobase

📄 Executing: 001_CreateTradeTable.sql
   ✅ Completed

📄 Executing: 002_CreateAdditionalTables.sql
   ✅ Completed

📊 Verifying tables...

Created tables:
  ✓ bea_table1
  ✓ bea_table2
  ✓ bea_table3
  ✓ trade
  ✓ trade_employment
  ✓ trade_factor
  ✓ trade_impact
  ✓ trade_material
  ✓ trade_resource

✅ All migrations completed successfully!
```

**What just happened:**
- 9 empty tables created in PostgreSQL
- Indexes added for performance
- Triggers set up for updated_at columns
- **NO DATA** in tables yet (0 rows in each)

### **3. Start the Web Application**
```bash
cd ../IndustryDB
dotnet run
```

**Expected output:**
```
Building...
info: Now listening on: http://localhost:5094
info: Application started. Press Ctrl+C to shut down.
info: CsvImportService initialized with path: ../trade-data
```

### **4. Import Data via Web UI**
```
1. Open browser: http://localhost:5094/TradeImport

2. Fill form:
   Year: [2022 ▼]
   Countries: US

3. Click "Create Database" button

4. Watch progress panel:
   Status: Processing
   Current Step: Processing US/imports
   Records Imported: 2
   [████████░░░░░░░░] 33%

5. When complete:
   Status: Completed
   Total records: 2
```

### **5. Verify Data in Database**
```bash
# Option A: Using Docker exec
docker exec -it trade-postgres psql -U postgres -d exiobase

# Then in psql:
SELECT * FROM public.trade;

# Expected output:
# trade_id | year | region1 | region2 | industry1   | industry2     | amount   | tradeflow_type | source_file
# ---------+------+---------+---------+-------------+---------------+----------+----------------+---------------------------
#        1 | 2022 | US      | CN      | Agriculture | Manufacturing | 1000.50  | imports        | 2022/US/imports/trade.csv
#        2 | 2022 | US      | MX      | Mining      | Transportation| 2000.75  | imports        | 2022/US/imports/trade.csv

# Check other tables (should be empty):
SELECT COUNT(*) FROM public.trade_employment;
# Expected: 0 (no CSV file exists)
```

---

## What Populates Each Table?

```
┌─────────────────────────────────────────────────────────────┐
│  CSV FILE                         →  DATABASE TABLE         │
├─────────────────────────────────────────────────────────────┤
│  trade.csv                        →  public.trade           │
│  trade_employment.csv             →  public.trade_employment│
│  trade_factor.csv                 →  public.trade_factor    │
│  trade_impact.csv                 →  public.trade_impact    │
│  trade_material.csv               →  public.trade_material  │
│  trade_resource.csv               →  public.trade_resource  │
│  bea_table1.csv (or bea1.csv)     →  public.bea_table1      │
│  bea_table2.csv (or bea2.csv)     →  public.bea_table2      │
│  bea_table3.csv (or bea3.csv)     →  public.bea_table3      │
└─────────────────────────────────────────────────────────────┘

MAPPING CODE: CsvImportService.GetTableNameFromFileName()
Location: IndustryDB/Services/CsvImportService.cs:125-144
```

---

## Summary: Your Current Test Options

| Option | Time | What You Get | Prerequisites |
|--------|------|--------------|---------------|
| **A. Quick Test** | 2 min | 2 rows in 1 table | Docker Desktop running |
| **B. Medium Test** | 30 min | ~10K rows in 9 tables | Python + exiobase repo |
| **C. Full Test** | 6 hours | ~500K rows, all countries | Lots of patience 😅 |

---

## Quick Decision Tree

```
Do you have Docker Desktop running?
├─ YES → Start with Scenario A (2 minutes)
│        ├─ Works? → Great! Optionally try Scenario B for more data
│        └─ Fails? → Check logs, troubleshoot
│
└─ NO → Start Docker Desktop first, then retry
```

---

## Common Questions

**Q: Why are most tables empty after Scenario A?**
A: Because you only have 1 CSV file (`trade.csv`). The other 8 CSV files don't exist yet.

**Q: How do I get the other CSV files?**
A: Run Python scripts (Scenario B) or manually create them following the same format.

**Q: Can I import just one table at a time?**
A: No, the import processes all CSV files it finds. But it gracefully skips missing files.

**Q: What if I want to test without Docker?**
A: You can use the Azure PostgreSQL database in `appsettings.json`, but migrations require access.

**Q: Do I need to run migrations every time?**
A: No! Only once. After that, tables exist and you can import/re-import data anytime.

---

## Next Steps

**Ready to test?** Follow Scenario A above!

Once Docker Desktop is running:
```bash
docker-compose up -d && cd DbMigrate && dotnet run && cd ../IndustryDB && dotnet run
```

Then open: http://localhost:5094/TradeImport 🚀
