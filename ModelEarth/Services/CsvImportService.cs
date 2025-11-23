using CsvHelper;
using CsvHelper.Configuration;
using ModelEarth.Models.Data;
using System.Globalization;

namespace ModelEarth.Services
{
    /// <summary>
    /// Service for reading and parsing CSV files from the trade-data repository.
    /// Handles file system operations and CSV parsing using CsvHelper library.
    /// </summary>
    public class CsvImportService : ICsvImportService
    {
        private readonly ILogger<CsvImportService> _logger;
        private readonly string _tradeDataPath;

        public CsvImportService(ILogger<CsvImportService> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Get trade data path from environment variable
            _tradeDataPath = Environment.GetEnvironmentVariable("TRADE_DATA_REPO_PATH")
                ?? throw new InvalidOperationException("TRADE_DATA_REPO_PATH environment variable not set");

            // Validate path exists
            if (!Directory.Exists(_tradeDataPath))
            {
                throw new DirectoryNotFoundException($"Trade data directory not found: {_tradeDataPath}");
            }

            _logger.LogInformation("CsvImportService initialized with path: {Path}", _tradeDataPath);
        }

        /// <summary>
        /// Gets all CSV files for a specific country, year, and tradeflow type.
        /// Example: GetCsvFilesForImport(2022, "US", "imports")
        /// Returns: ["trade.csv", "trade_employment.csv", ..., ~9 files total]
        /// </summary>
        public List<string> GetCsvFilesForImport(short year, string country, string tradeflowType)
        {
            var folder = Path.Combine(_tradeDataPath, "year", year.ToString(), country, tradeflowType);

            if (!Directory.Exists(folder))
            {
                _logger.LogWarning("CSV folder not found: {Folder}", folder);
                return new List<string>();
            }

            var csvFiles = Directory.GetFiles(folder, "*.csv", SearchOption.TopDirectoryOnly)
                .Where(f => !f.EndsWith("runnote.md", StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Found {Count} CSV files in {Folder}", csvFiles.Count, folder);
            return csvFiles;
        }

        /// <summary>
        /// Reads a CSV file and parses it into TradeImportRecord objects.
        /// </summary>
        public async Task<List<TradeImportRecord>> ReadCsvFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            var records = new List<TradeImportRecord>();

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null, // Ignore missing fields
                    HeaderValidated = null,   // Don't validate headers
                    TrimOptions = TrimOptions.Trim
                });

                await foreach (var record in csv.GetRecordsAsync<TradeImportRecord>())
                {
                    records.Add(record);
                }

                _logger.LogInformation("Read {Count} records from {File}", records.Count, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading CSV file: {File}", filePath);
                throw;
            }

            return records;
        }

        /// <summary>
        /// Gets all available countries for a specific year.
        /// Scans the trade-data/year/{year}/ directory for country folders.
        /// </summary>
        public List<string> GetAvailableCountries(short year)
        {
            var yearFolder = Path.Combine(_tradeDataPath, "year", year.ToString());

            if (!Directory.Exists(yearFolder))
            {
                _logger.LogWarning("Year folder not found: {Folder}", yearFolder);
                return new List<string>();
            }

            var countries = Directory.GetDirectories(yearFolder)
                .Select(d => new DirectoryInfo(d).Name)
                .Where(name => name.Length == 2) // Country codes are 2 letters
                .OrderBy(name => name)
                .ToList();

            _logger.LogInformation("Found {Count} countries for year {Year}: {Countries}",
                countries.Count, year, string.Join(", ", countries));

            return countries;
        }

        /// <summary>
        /// Determines the target database table name based on CSV filename.
        /// </summary>
        public string GetTableNameFromFileName(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName).ToLower();

            // Map CSV filenames to database table names
            return name switch
            {
                "trade" => "public.trade",
                "trade_employment" => "public.trade_employment",
                "trade_factor" => "public.trade_factor",
                "trade_impact" => "public.trade_impact",
                "trade_material" => "public.trade_material",
                "trade_resource" => "public.trade_resource",
                // BEA tables - update based on actual BEA CSV filenames
                "bea_table1" or "bea1" => "public.bea_table1",
                "bea_table2" or "bea2" => "public.bea_table2",
                "bea_table3" or "bea3" => "public.bea_table3",
                _ => throw new ArgumentException($"Unknown CSV file type: {fileName}")
            };
        }

        /// <summary>
        /// Validates that all expected CSV files exist for a country-year-tradeflow combination.
        /// </summary>
        public ValidationResult ValidateCsvFiles(short year, string country, string tradeflowType)
        {
            var result = new ValidationResult { IsValid = true };
            var csvFiles = GetCsvFilesForImport(year, country, tradeflowType);

            if (csvFiles.Count == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = $"No CSV files found for {year}/{country}/{tradeflowType}";
                return result;
            }

            // Expected minimum files (can be adjusted based on actual requirements)
            var expectedFiles = new[] { "trade.csv", "trade_employment.csv", "trade_factor.csv" };
            var fileNames = csvFiles.Select(f => Path.GetFileName(f).ToLower()).ToList();

            foreach (var expected in expectedFiles)
            {
                if (!fileNames.Contains(expected))
                {
                    result.Warnings.Add($"Expected file not found: {expected}");
                }
            }

            result.FileCount = csvFiles.Count;
            return result;
        }
    }

    /// <summary>
    /// Result of CSV file validation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public int FileCount { get; set; }
    }
}
