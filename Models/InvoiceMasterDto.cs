namespace TireInventory.Models
{
    // Projection DTO for InvoiceMaster with resolved FK names
    public class InvoiceMasterDto
    {
        public long Id { get; set; }
        public string tbim_Phone { get; set; } = string.Empty;
        public DateTime tbim_InvDate { get; set; }
        public string tbim_Name { get; set; } = string.Empty;
        public long? tbim_TaxId { get; set; }
        public string? tbim_VehicleMake { get; set; }
        public string? tbim_Model { get; set; }
        public string? tbim_Year { get; set; }
        public string? tbim_Odometer { get; set; }
        public decimal tbim_SubTotal { get; set; }
        public decimal tbim_SaleTax { get; set; }
        public decimal tbim_Labour { get; set; }
        public decimal tbim_DisPer { get; set; }
        public decimal tbim_DisAmt { get; set; }
        public decimal tbim_Total { get; set; }
        public decimal tbim_PaidAmt { get; set; }
        public string? tbim_PayInfo { get; set; }
        public string? tbim_Note { get; set; }
        public long? tbim_LocationDetailsId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SetDate { get; set; }

        // Resolved FK names
        public string LocationName { get; set; } = string.Empty;
        public string TaxCompanyName { get; set; } = string.Empty;
    }
}