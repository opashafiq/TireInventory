using System.Collections.Generic;

namespace TireInventory.Models
{
    // DTO used by CreateLayawayRefund endpoint
    public class CreateLayawayRefundDto
    {
        public LayawayRefundMaster Refund { get; set; } = new LayawayRefundMaster();
        public List<LayawayRefundDetails> RefundDetails { get; set; } = new List<LayawayRefundDetails>();
        public List<LayawayRefundPayments> RefundPayments { get; set; } = new List<LayawayRefundPayments>();
    }
}