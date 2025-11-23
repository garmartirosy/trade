using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelEarth.Controllers;
using ModelEarth.Models.Data;
using ModelEarth.Services;
using Moq;
using Xunit;

namespace ModelEarth.Tests.Controllers
{
    public class TradeImportControllerTests
    {
        private readonly Mock<ICsvImportService> _mockCsvService;
        private readonly Mock<ITradeDataRepository> _mockRepository;
        private readonly Mock<ILogger<TradeImportController>> _mockLogger;
        private readonly TradeImportController _controller;

        public TradeImportControllerTests()
        {
            _mockCsvService = new Mock<ICsvImportService>();
            _mockRepository = new Mock<ITradeDataRepository>();
            _mockLogger = new Mock<ILogger<TradeImportController>>();

            _controller = new TradeImportController(
                _mockCsvService.Object,
                _mockRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateDatabase_Should_Return_BadRequest_When_Year_Is_Invalid()
        {
            // Arrange
            var request = new DatabaseCreationRequest
            {
                Year = 2050, // Invalid year
                Countries = null,
                ClearExistingData = false
            };

            // Act
            var result = await _controller.CreateDatabase(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().NotBeNull();
        }

        [Theory]
        [InlineData(2018)]
        [InlineData(2031)]
        [InlineData(1990)]
        public async Task CreateDatabase_Should_Return_BadRequest_For_Year_OutOfRange(short year)
        {
            // Arrange
            var request = new DatabaseCreationRequest
            {
                Year = year,
                Countries = null,
                ClearExistingData = false
            };

            // Act
            var result = await _controller.CreateDatabase(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateDatabase_Should_Return_Ok_When_Request_Is_Valid()
        {
            // Arrange
            var request = new DatabaseCreationRequest
            {
                Year = 2022,
                Countries = new[] { "US", "IN" },
                ClearExistingData = false
            };

            // Act
            var result = await _controller.CreateDatabase(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();

            // Verify the response contains expected properties
            var response = okResult?.Value;
            response.Should().NotBeNull();

            var jobIdProperty = response?.GetType().GetProperty("jobId");
            jobIdProperty.Should().NotBeNull();

            var yearProperty = response?.GetType().GetProperty("year");
            yearProperty.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateDatabase_Should_Start_Background_Job()
        {
            // Arrange
            var request = new DatabaseCreationRequest
            {
                Year = 2022,
                Countries = new[] { "US" },
                ClearExistingData = false
            };

            // Act
            var result = await _controller.CreateDatabase(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            // Give background task a moment to start
            await Task.Delay(100);

            // The background job should have been initiated (we can't easily test the execution itself in a unit test)
        }

        [Fact]
        public async Task GetImportStatus_Should_Return_NotFound_When_JobId_Invalid()
        {
            // Arrange
            var invalidJobId = Guid.NewGuid().ToString();

            // Act
            var result = _controller.GetImportStatus(invalidJobId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetStatistics_Should_Return_Ok_With_Statistics()
        {
            // Arrange
            var year = (short)2022;
            _mockRepository.Setup(x => x.GetImportStatisticsAsync(year))
                .ReturnsAsync(new List<ImportStatistics>
                {
                    new ImportStatistics
                    {
                        region1 = "US",
                        tradeflow_type = "imports",
                        trade_count = 5000,
                        total_amount = 1000000m
                    }
                });

            _mockRepository.Setup(x => x.GetTableCountsAsync(year))
                .ReturnsAsync(new List<TableCount>
                {
                    new TableCount { table_name = "trade", row_count = 5000 }
                });

            _mockRepository.Setup(x => x.GetDistinctCountriesAsync(year))
                .ReturnsAsync(new List<CountryInfo>
                {
                    new CountryInfo { country_code = "US", tradeflow_count = 3, total_trade_records = 15000 }
                });

            // Act
            var result = await _controller.GetStatistics(year);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();

            _mockRepository.Verify(x => x.GetImportStatisticsAsync(year), Times.Once);
            _mockRepository.Verify(x => x.GetTableCountsAsync(year), Times.Once);
            _mockRepository.Verify(x => x.GetDistinctCountriesAsync(year), Times.Once);
        }

        [Fact]
        public async Task GetStatistics_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var year = (short)2022;
            _mockRepository.Setup(x => x.GetImportStatisticsAsync(year))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetStatistics(year);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task TestConnection_Should_Return_Ok_With_Connection_Status()
        {
            // Arrange
            _mockRepository.Setup(x => x.TestConnectionAsync())
                .ReturnsAsync(true);

            // Act
            var result = await _controller.TestConnection();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();

            var connectedProperty = okResult?.Value?.GetType().GetProperty("connected");
            connectedProperty.Should().NotBeNull();

            _mockRepository.Verify(x => x.TestConnectionAsync(), Times.Once);
        }

        [Fact]
        public async Task TestConnection_Should_Return_False_When_Connection_Fails()
        {
            // Arrange
            _mockRepository.Setup(x => x.TestConnectionAsync())
                .ReturnsAsync(false);

            // Act
            var result = await _controller.TestConnection();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            var value = okResult?.Value;
            var connectedProperty = value?.GetType().GetProperty("connected");
            var connectedValue = connectedProperty?.GetValue(value);

            connectedValue.Should().Be(false);
        }
    }
}
