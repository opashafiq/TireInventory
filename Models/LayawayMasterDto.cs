namespace TireInventory.Models
{
    public class LayawayMasterDto
    {
        public long Id { get; set; }
        public string tbim_Phone { get; set; } = string.Empty;
        public DateTime tbim_InvDate { get; set; }
        public string tbim_Name { get; set; } = string.Empty;
        public long? tbim_TaxId { get; set; }
        public string? tbim_VehicleMake { get; set; }
        public string? tbim_Model { get; set; }
        public string? tbim_Year { get; set; }
        public decimal tbim_SubTotal { get; set; }
        public decimal tbim_SaleTax { get; set; }
        public decimal tbim_Total { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SetDate { get; set; }

        // resolved names
        public string LocationName { get; set; } = string.Empty;
        public string TaxCompanyName { get; set; } = string.Empty;
    }
}