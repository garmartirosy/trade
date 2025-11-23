namespace ModelEarth.Models.Data
{
    /// <summary>
    /// Represents one row from a CSV file before it's converted to a Trade object.
    /// This is a temporary DTO (Data Transfer Object) used during CSV parsing.
    /// </summary>
    public class TradeImportRecord
    {
        public string Region1 { get; set; } = "";
        public string Region2 { get; set; } = "";
        public string Industry1 { get; set; } = "";
        public string Industry2 { get; set; } = "";
        public decimal Amount { get; set; }

        // Note: Year, TradeflowType, and SourceFile are added during processing,
        // not read from the CSV file
    }
}
