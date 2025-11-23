using ModelEarth.Models.Data;

namespace ModelEarth.Services
{
    /// <summary>
    /// Interface for CSV import service operations.
    /// Handles reading and parsing CSV files from the trade-data repository.
    /// </summary>
    public interface ICsvImportService
    {
        /// <summary>
        /// Gets all CSV files for a specific country, year, and tradeflow type.
        /// </summary>
        List<string> GetCsvFilesForImport(short year, string country, string tradeflowType);

        /// <summary>
        /// Reads a CSV file and parses it into TradeImportRecord objects.
        /// </summary>
        Task<List<TradeImportRecord>> ReadCsvFileAsync(string filePath);

        /// <summary>
        /// Gets all available countries for a specific year.
        /// </summary>
        List<string> GetAvailableCountries(short year);

        /// <summary>
        /// Determines the target database table name based on CSV filename.
        /// </summary>
        string GetTableNameFromFileName(string fileName);

        /// <summary>
        /// Validates that all expected CSV files exist for a country-year-tradeflow combination.
        /// </summary>
        ValidationResult ValidateCsvFiles(short year, string country, string tradeflowType);
    }
}
