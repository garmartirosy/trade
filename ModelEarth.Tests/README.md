# ModelEarth.Tests

Unit and integration tests for the Trade Data Import System.

## Test Structure

```
ModelEarth.Tests/
├── Controllers/
│   └── TradeImportControllerTests.cs      # API controller tests
├── Services/
│   ├── CsvImportServiceTests.cs           # CSV parsing tests
│   └── TradeDataRepositoryTests.cs        # Database repository tests
├── Integration/
│   └── CsvImportIntegrationTests.cs       # End-to-end integration tests
└── TestData/
    └── sample_trade.csv                   # Sample CSV for testing
```

## Running Tests

### Run All Tests
```bash
cd ModelEarth.Tests
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test --filter "Category!=Integration"
```

### Run Integration Tests Only
```bash
dotnet test --filter "Category=Integration"
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Categories

### Unit Tests
- **CsvImportServiceTests**: Tests CSV file discovery, reading, parsing, and validation
- **TradeDataRepositoryTests**: Tests SQL generation and database operations
- **TradeImportControllerTests**: Tests API endpoints and request validation

### Integration Tests
- **CsvImportIntegrationTests**: Tests complete import workflow with real file system operations

## Test Coverage

The test suite covers:
- ✅ CSV file discovery and validation
- ✅ CSV parsing with CsvHelper
- ✅ Data type conversions
- ✅ API request validation
- ✅ Error handling
- ✅ Background job initiation
- ✅ Database connection testing
- ✅ End-to-end import flow

## Testing Best Practices

1. **Use Arrange-Act-Assert pattern** in all tests
2. **Mock external dependencies** (database, file system in unit tests)
3. **Use FluentAssertions** for readable assertions
4. **Mark integration tests** with `[Trait("Category", "Integration")]`
5. **Clean up test resources** in Dispose() methods
6. **Use realistic test data** that matches production CSV structure

## Sample Test Data

The `TestData/` folder contains sample CSV files for testing:
- `sample_trade.csv` - Sample trade flow data

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Unit tests: Fast, no external dependencies
- Integration tests: Require file system access only (no database)

## Future Enhancements

- [ ] Add database integration tests (requires test PostgreSQL instance)
- [ ] Add performance tests for large CSV files
- [ ] Add API integration tests with TestServer
- [ ] Add code coverage reporting
