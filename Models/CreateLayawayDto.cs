using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateLayaway endpoint
    public class CreateLayawayDto
    {
        public LayawayMasterDto LayawayMasterDto { get; set; } = new LayawayMasterDto();
        public List<LayawayDetailsDto> LayawayDetailsDto { get; set; } = new List<LayawayDetailsDto>();
        public List<LayawayPaymentsDto> LayawayPaymentsDto { get; set; } = new List<LayawayPaymentsDto>();
    }
}