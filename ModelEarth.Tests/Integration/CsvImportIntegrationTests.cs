using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelEarth.Models.Data;
using ModelEarth.Services;
using Moq;
using Xunit;

namespace ModelEarth.Tests.Integration
{
    /// <summary>
    /// Integration tests that test the complete CSV import flow.
    /// These tests use real file system operations but mock database connections.
    /// </summary>
    [Trait("Category", "Integration")]
    public class CsvImportIntegrationTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly Mock<ILogger<CsvImportService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public CsvImportIntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), "trade-integration-test", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDataPath);

            _mockLogger = new Mock<ILogger<CsvImportService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", _testDataPath);

            // Create realistic test CSV structure
            CreateTestCsvStructure();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", null);
        }

        private void CreateTestCsvStructure()
        {
            // Create structure: year/2022/US/imports/*.csv
            var usImportsPath = Path.Combine(_testDataPath, "year", "2022", "US", "imports");
            Directory.CreateDirectory(usImportsPath);

            // Create trade.csv
            var tradeCsv = @"Region1,Region2,Industry1,Industry2,Amount
US,CN,Agriculture,Manufacturing,1000000.50
US,MX,Mining,Transportation,500000.00
US,CA,Services,Technology,750000.25";
            File.WriteAllText(Path.Combine(usImportsPath, "trade.csv"), tradeCsv);

            // Create trade_employment.csv
            var employmentCsv = @"Region1,Region2,Industry1,Industry2,Amount
US,CN,Agriculture,Manufacturing,500.00
US,MX,Mining,Transportation,250.00";
            File.WriteAllText(Path.Combine(usImportsPath, "trade_employment.csv"), employmentCsv);

            // Create exports folder
            var usExportsPath = Path.Combine(_testDataPath, "year", "2022", "US", "exports");
            Directory.CreateDirectory(usExportsPath);
            File.WriteAllText(Path.Combine(usExportsPath, "trade.csv"), tradeCsv);

            // Create another country (IN)
            var inImportsPath = Path.Combine(_testDataPath, "year", "2022", "IN", "imports");
            Directory.CreateDirectory(inImportsPath);
            File.WriteAllText(Path.Combine(inImportsPath, "trade.csv"), tradeCsv);
        }

        [Fact]
        public void End_To_End_Import_Flow_Should_Work()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act & Assert - Step 1: Get available countries
            var countries = service.GetAvailableCountries(2022);
            countries.Should().Contain("US");
            countries.Should().Contain("IN");

            // Step 2: Get CSV files for US imports
            var csvFiles = service.GetCsvFilesForImport(2022, "US", "imports");
            csvFiles.Should().HaveCount(2); // trade.csv and trade_employment.csv

            // Step 3: Validate CSV files
            var validation = service.ValidateCsvFiles(2022, "US", "imports");
            validation.IsValid.Should().BeTrue();
            validation.FileCount.Should().Be(2);

            // Step 4: Read CSV file
            var tradeCsvFile = csvFiles.First(f => f.EndsWith("trade.csv"));
            var records = service.ReadCsvFileAsync(tradeCsvFile).Result;
            records.Should().HaveCount(3);

            // Step 5: Verify data mapping
            records[0].Region1.Should().Be("US");
            records[0].Region2.Should().Be("CN");
            records[0].Amount.Should().Be(1000000.50m);

            // Step 6: Get table name from filename
            var tableName = service.GetTableNameFromFileName("trade.csv");
            tableName.Should().Be("public.trade");
        }

        [Fact]
        public async Task Should_Handle_Multiple_Countries_And_Tradeflows()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var countries = service.GetAvailableCountries(2022);

            // Assert
            countries.Should().HaveCount(2); // US and IN

            foreach (var country in countries)
            {
                // Check imports
                var importFiles = service.GetCsvFilesForImport(2022, country, "imports");
                importFiles.Should().NotBeEmpty();

                foreach (var file in importFiles)
                {
                    var records = await service.ReadCsvFileAsync(file);
                    records.Should().NotBeEmpty();
                    records.Should().AllSatisfy(r =>
                    {
                        r.Region1.Should().NotBeNullOrWhiteSpace();
                        r.Region2.Should().NotBeNullOrWhiteSpace();
                        r.Industry1.Should().NotBeNullOrWhiteSpace();
                        r.Industry2.Should().NotBeNullOrWhiteSpace();
                        r.Amount.Should().BeGreaterThan(0);
                    });
                }
            }
        }

        [Fact]
        public async Task Should_Convert_ImportRecords_To_Trade_Objects_Correctly()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var csvFiles = service.GetCsvFilesForImport(2022, "US", "imports");

            if (!csvFiles.Any(f => f.EndsWith("trade.csv")))
            {
                // Skip test if no trade.csv exists
                return;
            }

            var csvFile = csvFiles.First(f => f.EndsWith("trade.csv"));

            // Act
            var importRecords = await service.ReadCsvFileAsync(csvFile);

            // Convert to Trade objects (simulating controller logic)
            var trades = importRecords.Select(r => new Trade
            {
                Year = 2022,
                Region1 = r.Region1,
                Region2 = r.Region2,
                Industry1 = r.Industry1,
                Industry2 = r.Industry2,
                Amount = r.Amount,
                TradeflowType = "imports",
                SourceFile = "2022/US/imports/trade.csv"
            }).ToList();

            // Assert
            trades.Should().HaveCount(3);
            trades.Should().AllSatisfy(t =>
            {
                t.Year.Should().Be(2022);
                t.TradeflowType.Should().Be("imports");
                t.SourceFile.Should().Be("2022/US/imports/trade.csv");
            });

            trades[0].Region1.Should().Be("US");
            trades[0].Region2.Should().Be("CN");
            trades[0].Amount.Should().Be(1000000.50m);
        }

        [Fact]
        public void Should_Handle_Missing_Tradeflow_Folder_Gracefully()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var csvFiles = service.GetCsvFilesForImport(2022, "US", "domestic"); // Doesn't exist

            // Assert
            csvFiles.Should().BeEmpty();

            var validation = service.ValidateCsvFiles(2022, "US", "domestic");
            validation.IsValid.Should().BeFalse();
            validation.ErrorMessage.Should().Contain("No CSV files found");
        }

        [Fact]
        public async Task Should_Handle_Large_CSV_Files()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var largeCsvPath = Path.Combine(_testDataPath, "year", "2022", "US", "imports", "large_trade.csv");

            // Create a large CSV file with 10,000 rows
            using (var writer = new StreamWriter(largeCsvPath))
            {
                writer.WriteLine("Region1,Region2,Industry1,Industry2,Amount");
                for (int i = 0; i < 10000; i++)
                {
                    writer.WriteLine($"US,CN,Industry{i % 100},Industry{(i + 1) % 100},{1000.50 + i}");
                }
            }

            // Act
            var records = await service.ReadCsvFileAsync(largeCsvPath);

            // Assert
            records.Should().HaveCount(10000);
            records.All(r => r.Amount > 0).Should().BeTrue();
        }
    }
}
