namespace TireInventory.Models.ReportDtos
{
    public class PendingAmountDto
    {
        public string? tbip_Paymentname { get; set; }
        public long Id { get; set; }
        public long tbim_InvoiceIdRad { get; set; }
        public string? tbim_Name { get; set; }
        public string? tbti_ComName { get; set; }
        public string tbim_InvDate { get; set; }
        public decimal tbim_Total { get; set; }
        public decimal tbim_PaidAmt { get; set; }
        public string? tbim_PayInfo { get; set; }
        public string? PayInfo { get; set; }
    }
}
