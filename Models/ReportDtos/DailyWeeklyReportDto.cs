namespace TireInventory.Models.ReportDtos
{
    public class DailyWeeklyReportDto
    {
        public string Date { get; set; }
        public decimal Credit { get; set; }
        public decimal Cash { get; set; }
        public decimal CreditCardVisa { get; set; }
        public decimal DEBITCARD { get; set; }
        public decimal CHECKElectReg { get; set; }
        public decimal CreditCardMaster { get; set; }
        public decimal CreditCardDisc { get; set; }
        public decimal CreditCardAmex { get; set; }
        public decimal CrestFinance { get; set; }
        public decimal ProgFinance { get; set; }
    }
}
