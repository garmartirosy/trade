namespace ModelEarth.Models.Data
{
    /// <summary>
    /// Tracks the status of CSV file imports.
    /// Used to monitor progress and log import results.
    /// </summary>
    public class ImportStatus
    {
        public int Id { get; set; }
        public short Year { get; set; }
        public string Country { get; set; } = "";             // e.g., "US", "IN"
        public string TradeflowType { get; set; } = "";       // "imports", "exports", "domestic"
        public string TableName { get; set; } = "";           // "trade", "trade_employment", etc.
        public string FileName { get; set; } = "";            // e.g., "trade.csv"
        public int RecordsImported { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "Running";       // "Running", "Completed", "Failed"
        public string? ErrorMessage { get; set; }
    }
}
