namespace ModelEarth.Models.Data
{
    /// <summary>
    /// Request model for the "Create Database" API endpoint.
    /// Sent from the frontend when user clicks the "Create Database" button.
    /// </summary>
    public class DatabaseCreationRequest
    {
        /// <summary>
        /// The year to import (e.g., 2019, 2022)
        /// </summary>
        public short Year { get; set; }

        /// <summary>
        /// Optional array of country codes to import.
        /// If null or empty, imports ALL countries available in the CSV files.
        /// Example: ["US", "IN", "CN"]
        /// </summary>
        public string[]? Countries { get; set; }

        /// <summary>
        /// If true, deletes all existing data for this year before importing.
        /// If false, appends to existing data (may cause duplicates).
        /// </summary>
        public bool ClearExistingData { get; set; } = false;
    }
}
