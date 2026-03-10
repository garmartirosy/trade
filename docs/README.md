# Trade Data Import System - Documentation

Complete documentation for the CSV-to-SQL trade data import system.

## 📚 Quick Navigation

### Getting Started
- **[Getting Started Guide](guides/GETTING_STARTED.md)** - Installation, setup, and first import
- **[Complete Test Flow](guides/COMPLETE_TEST_FLOW.md)** - End-to-end testing scenarios
- **[Start Database](guides/START_DATABASE.md)** - Docker PostgreSQL setup
- **[WebRoot Setup](guides/WEBROOT_SETUP_EXPLAINED.md)** - IndustryDB ecosystem overview

### Technical Documentation
- **[System Architecture](ARCHITECTURE.md)** - Complete architecture with diagrams
- **[.NET Code Guide](guides/DOTNET_CODE_GUIDE.md)** - Comprehensive .NET 8.0 codebase guide
- **[Testing Guide](testing/TESTING_GUIDE.md)** - All 35 tests explained

## 🚀 Quick Start

```bash
# 1. Setup database
docker-compose up -d

# 2. Run migrations
cd DbMigrate && dotnet run

# 3. Start application
cd ../IndustryDB && dotnet run

# 4. Open browser
http://localhost:5094/TradeImport
```

## 📊 System Overview

**What it does:** Imports 405 CSV files (15 countries × 3 tradeflows × 9 file types) into 9 PostgreSQL tables.

**Key Features:**
- ✅ 35/35 tests passing
- ✅ Interface-based DI architecture
- ✅ Background job processing
- ✅ Real-time progress tracking
- ✅ Batch inserts (1000 records/batch)

## 📖 Documentation Structure

```
docs/
├── README.md                    (This file)
├── ARCHITECTURE.md              (System architecture)
├── guides/
│   ├── GETTING_STARTED.md       (Setup guide)
│   ├── DOTNET_CODE_GUIDE.md     (.NET 8.0 codebase guide)
│   ├── COMPLETE_TEST_FLOW.md    (Testing scenarios)
│   ├── START_DATABASE.md        (Docker setup)
│   └── WEBROOT_SETUP_EXPLAINED.md (IndustryDB ecosystem)
└── testing/
    └── TESTING_GUIDE.md         (Test documentation)
```

## 🔗 External Links

- **Main README**: [../README.md](../README.md)
- **GitHub Issue #30**: https://github.com/modelearth/trade/issues/30
- **CSV Data Source**: https://github.com/IndustryDB/trade-data
- **Trade Visualization**: https://model.earth/profile/footprint/

## 💡 Need Help?

1. **Installation issues?** → [Getting Started - Troubleshooting](guides/GETTING_STARTED.md#troubleshooting)
2. **Test failures?** → [Testing Guide](testing/TESTING_GUIDE.md)
3. **Architecture questions?** → [System Architecture](ARCHITECTURE.md)
4. **API questions?** → See [README.md](../README.md#api-endpoints)
