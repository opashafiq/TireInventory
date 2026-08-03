namespace TireInventory.Models
{
    public class BulkImportResultDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
