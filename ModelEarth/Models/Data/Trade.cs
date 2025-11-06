namespace ModelEarth.Models.Data
{
    public class Trade
    {
        public long TradeId { get; set; }       // bigint
        public short Year { get; set; }         // smallint

        public string Region1 { get; set; } = "";  // char(2)
        public string Region2 { get; set; } = "";  // char(2)

        public string Industry1 { get; set; } = "";  // text
        public string Industry2 { get; set; } = "";  // text

        public decimal Amount { get; set; }     // numeric(20,*)
    }

}
