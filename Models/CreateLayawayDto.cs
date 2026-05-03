using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateLayaway endpoint
    public class CreateLayawayDto
    {
        public LayawayMaster Layaway { get; set; } = new LayawayMaster();
        public List<LayawayDetails> LayawayDetails { get; set; } = new List<LayawayDetails>();
        public List<LayawayPayments> LayawayPayments { get; set; } = new List<LayawayPayments>();
    }
}