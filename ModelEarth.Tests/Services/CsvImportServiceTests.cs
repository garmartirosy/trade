using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelEarth.Models.Data;
using ModelEarth.Services;
using Moq;
using Xunit;

namespace ModelEarth.Tests.Services
{
    public class CsvImportServiceTests : IDisposable
    {
        private readonly Mock<ILogger<CsvImportService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly string _testDataPath;

        public CsvImportServiceTests()
        {
            _mockLogger = new Mock<ILogger<CsvImportService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Create temporary test directory
            _testDataPath = Path.Combine(Path.GetTempPath(), "trade-test-data", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDataPath);

            // Set environment variable for test
            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", _testDataPath);
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", null);
        }

        [Fact]
        public void Constructor_Should_Throw_When_TradeDataPath_NotSet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                new CsvImportService(_mockLogger.Object, _mockConfiguration.Object));

            exception.Message.Should().Contain("TRADE_DATA_REPO_PATH");
        }

        [Fact]
        public void Constructor_Should_Throw_When_TradeDataPath_DoesNotExist()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TRADE_DATA_REPO_PATH", "C:/nonexistent/path");

            // Act & Assert
            var exception = Assert.Throws<DirectoryNotFoundException>(() =>
                new CsvImportService(_mockLogger.Object, _mockConfiguration.Object));

            exception.Message.Should().Contain("Trade data directory not found");
        }

        [Fact]
        public void GetCsvFilesForImport_Should_Return_Empty_When_Folder_DoesNotExist()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var files = service.GetCsvFilesForImport(2022, "XX", "imports");

            // Assert
            files.Should().BeEmpty();
        }

        [Fact]
        public void GetCsvFilesForImport_Should_Return_CsvFiles_When_Folder_Exists()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var testFolder = Path.Combine(_testDataPath, "year", "2022", "US", "imports");
            Directory.CreateDirectory(testFolder);

            // Create test CSV files
            File.WriteAllText(Path.Combine(testFolder, "trade.csv"), "header\ndata");
            File.WriteAllText(Path.Combine(testFolder, "trade_employment.csv"), "header\ndata");
            File.WriteAllText(Path.Combine(testFolder, "runnote.md"), "notes"); // Should be ignored

            // Act
            var files = service.GetCsvFilesForImport(2022, "US", "imports");

            // Assert
            files.Should().HaveCount(2);
            files.Should().Contain(f => f.EndsWith("trade.csv"));
            files.Should().Contain(f => f.EndsWith("trade_employment.csv"));
            files.Should().NotContain(f => f.EndsWith("runnote.md"));
        }

        [Fact]
        public async Task ReadCsvFileAsync_Should_Throw_When_File_DoesNotExist()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var nonExistentFile = Path.Combine(_testDataPath, "nonexistent.csv");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                service.ReadCsvFileAsync(nonExistentFile));
        }

        [Fact]
        public async Task ReadCsvFileAsync_Should_Parse_Valid_CsvFile()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var csvFile = Path.Combine(_testDataPath, "test.csv");

            var csvContent = @"Region1,Region2,Industry1,Industry2,Amount
US,CN,Agriculture,Manufacturing,1000.50
US,MX,Mining,Transportation,2000.75";

            File.WriteAllText(csvFile, csvContent);

            // Act
            var records = await service.ReadCsvFileAsync(csvFile);

            // Assert
            records.Should().HaveCount(2);
            records[0].Region1.Should().Be("US");
            records[0].Region2.Should().Be("CN");
            records[0].Industry1.Should().Be("Agriculture");
            records[0].Industry2.Should().Be("Manufacturing");
            records[0].Amount.Should().Be(1000.50m);

            records[1].Region1.Should().Be("US");
            records[1].Region2.Should().Be("MX");
            records[1].Amount.Should().Be(2000.75m);
        }

        [Fact]
        public void GetAvailableCountries_Should_Return_Empty_When_Year_DoesNotExist()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var countries = service.GetAvailableCountries(2022);

            // Assert
            countries.Should().BeEmpty();
        }

        [Fact]
        public void GetAvailableCountries_Should_Return_Country_Codes()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var yearFolder = Path.Combine(_testDataPath, "year", "2022");
            Directory.CreateDirectory(Path.Combine(yearFolder, "US"));
            Directory.CreateDirectory(Path.Combine(yearFolder, "IN"));
            Directory.CreateDirectory(Path.Combine(yearFolder, "CN"));
            Directory.CreateDirectory(Path.Combine(yearFolder, "InvalidFolder")); // Should be ignored (not 2 chars)

            // Act
            var countries = service.GetAvailableCountries(2022);

            // Assert
            countries.Should().HaveCount(3);
            countries.Should().Contain("US");
            countries.Should().Contain("IN");
            countries.Should().Contain("CN");
            countries.Should().NotContain("InvalidFolder");
        }

        [Theory]
        [InlineData("trade.csv", "public.trade")]
        [InlineData("trade_employment.csv", "public.trade_employment")]
        [InlineData("trade_factor.csv", "public.trade_factor")]
        [InlineData("trade_impact.csv", "public.trade_impact")]
        [InlineData("trade_material.csv", "public.trade_material")]
        [InlineData("trade_resource.csv", "public.trade_resource")]
        public void GetTableNameFromFileName_Should_Map_Correctly(string fileName, string expectedTable)
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var tableName = service.GetTableNameFromFileName(fileName);

            // Assert
            tableName.Should().Be(expectedTable);
        }

        [Fact]
        public void GetTableNameFromFileName_Should_Throw_For_Unknown_File()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                service.GetTableNameFromFileName("unknown.csv"));

            exception.Message.Should().Contain("Unknown CSV file type");
        }

        [Fact]
        public void ValidateCsvFiles_Should_Return_Invalid_When_No_Files_Found()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = service.ValidateCsvFiles(2022, "US", "imports");

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No CSV files found");
        }

        [Fact]
        public void ValidateCsvFiles_Should_Return_Valid_With_Warnings_When_Expected_Files_Missing()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var testFolder = Path.Combine(_testDataPath, "year", "2022", "US", "imports");
            Directory.CreateDirectory(testFolder);

            // Create only one CSV file (missing expected files)
            File.WriteAllText(Path.Combine(testFolder, "trade_impact.csv"), "header\ndata");

            // Act
            var result = service.ValidateCsvFiles(2022, "US", "imports");

            // Assert
            result.IsValid.Should().BeTrue();
            result.FileCount.Should().Be(1);
            result.Warnings.Should().Contain(w => w.Contains("trade.csv"));
            result.Warnings.Should().Contain(w => w.Contains("trade_employment.csv"));
        }

        [Fact]
        public void ValidateCsvFiles_Should_Return_Valid_When_All_Expected_Files_Present()
        {
            // Arrange
            var service = new CsvImportService(_mockLogger.Object, _mockConfiguration.Object);
            var testFolder = Path.Combine(_testDataPath, "year", "2022", "US", "imports");
            Directory.CreateDirectory(testFolder);

            // Create expected CSV files
            File.WriteAllText(Path.Combine(testFolder, "trade.csv"), "header\ndata");
            File.WriteAllText(Path.Combine(testFolder, "trade_employment.csv"), "header\ndata");
            File.WriteAllText(Path.Combine(testFolder, "trade_factor.csv"), "header\ndata");

            // Act
            var result = service.ValidateCsvFiles(2022, "US", "imports");

            // Assert
            result.IsValid.Should().BeTrue();
            result.FileCount.Should().Be(3);
            result.Warnings.Should().BeEmpty();
        }
    }
}
