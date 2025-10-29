namespace PayrollAnalytics.Api.DTOs
{
    public class Kpis
    {
        public decimal OverallCost { get; set; }
        public decimal OvertimeCost { get; set; }
        public double OvertimePct { get; set; }
        public decimal AverageSalary { get; set; }
        public int Headcount { get; set; }
        public int NewHires { get; set; }
        public int Terminations { get; set; }
        public decimal TurnoverCost { get; set; }
        public double TurnoverRate { get; set; }
    }
}
