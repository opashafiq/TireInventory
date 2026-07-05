using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateLayawayRefund endpoint
    public class CreateLayawayRefundDto
    {
        public LayawayRefundMasterDto layawayRefundMasterDto { get; set; } = new LayawayRefundMasterDto();
        public List<LayawayRefundDetailsDto> layawayRefundDetailsDto { get; set; } = new List<LayawayRefundDetailsDto>();
        public List<LayawayRefundPaymentsDto> layawayRefundPaymentsDto { get; set; } = new List<LayawayRefundPaymentsDto>();
    }
}