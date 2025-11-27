using ModelEarth.Models.Data;

namespace ModelEarth.Services
{
    /// <summary>
    /// Interface for trade data repository operations.
    /// Handles bulk inserts, deletes, and statistics using PostgreSQL.
    /// </summary>
    public interface ITradeDataRepository
    {
        /// <summary>
        /// Bulk insert trade records into the specified table with batching for performance.
        /// </summary>
        Task<int> BulkInsertAsync(IEnumerable<Trade> records, string tableName, int batchSize = 1000);

        /// <summary>
        /// Clears all data for a specific year across all tables.
        /// Calls the PostgreSQL stored procedure clear_year_data().
        /// </summary>
        Task<Dictionary<string, int>> ClearYearDataAsync(short year);

        /// <summary>
        /// Gets import statistics grouped by region and tradeflow type for a specific year.
        /// </summary>
        Task<List<ImportStatistics>> GetImportStatisticsAsync(short year);

        /// <summary>
        /// Gets row counts for all trade-related tables, optionally filtered by year.
        /// </summary>
        Task<List<TableCount>> GetTableCountsAsync(short? year = null);

        /// <summary>
        /// Gets distinct countries that have data for a specific year.
        /// </summary>
        Task<List<CountryInfo>> GetDistinctCountriesAsync(short year);

        /// <summary>
        /// Tests the database connection.
        /// </summary>
        Task<bool> TestConnectionAsync();
    }
}
