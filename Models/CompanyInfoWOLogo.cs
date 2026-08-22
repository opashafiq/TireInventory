namespace TireInventory.Models
{
    public class CompanyInfoWOLogo
    {
        public long Id { get; set; }

        public string tbbiBusinessName { get; set; } = null!;

        public string? tbbi_Address1 { get; set; }

        public string? tbbi_Address2 { get; set; }

        public string? tbbi_City { get; set; }

        public string? tbbi_State { get; set; }

        public string? tbbi_ZipCode { get; set; }

        public string? tbbi_Country { get; set; }

        public string? tbbi_Phone { get; set; }

        public string? tbbi_Fax { get; set; }

        public string? tbbi_Email { get; set; }

        public string UserName { get; set; } = null!;

        public DateTime SetDate { get; set; }
    }
}
