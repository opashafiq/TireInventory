namespace TireInventory.Models
{
    // Projection DTO for LocationDetails with resolved CompanyInfo name
    public class LocationDetailsDto
    {
        public long Id { get; set; }
        public string tbld_LocationName { get; set; } = string.Empty;
        public long CompanyInfoId { get; set; }
        public string? tbld_Address1 { get; set; }
        public string? tbld_Address2 { get; set; }
        public string? tbld_City { get; set; }
        public string? tbld_State { get; set; }
        public string? tbld_ZipCode { get; set; }
        public string? tbld_Phone { get; set; }
        public string? tbld_Fax { get; set; }
        public string? tbld_Email { get; set; }
        public string UserName { get; set; } = null!;

        public DateTime SetDate { get; set; }
        public string CompanyName { get; set; } = string.Empty; // resolved name
    }
}