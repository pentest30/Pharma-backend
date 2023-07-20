namespace GHPCommerce.Modules.Sales.DTOs
{
    public class TurnoverDto
    {
        public string CustomerName { get; set; }
        public decimal DailyTurnover { get; set; }
        public decimal MonthlyTurnover { get; set; }
        public decimal QuarterTurnover { get; set; }
        public decimal Turnover { get; set; }
        public string ReportType { get; set; }
        public decimal? MonthlyObjective { get; set; }
        public string SalesPersonName { get; set; }
        public string SalesGroup { get; set; }

    }
}