using System;
using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Models
{
    /// <summary>
    /// Order.
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }
        public int OrderChangeRequestId { get; set; }
        public string NHMemberId { get; set; }
        public List<ItemDetail> ItemDetails { get; set; }
        public string CarrierName { get; set; }
        public List<ItemComment> ItemComments { get; set; }
        public string UserName { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime RequestedDate { get; set; }
        public string RequestType { get; set; }
        public string Status { get; set; }
        public string AdminComments { get; set; }
        public decimal TotalPrice { get; set; }
        public string IsProcessed { get; set; }
        public string TicketId { get; set; }
    }

    /// <summary>
    /// Item detail.
    /// </summary>
    public class ItemDetail
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public long OrderItemId { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// Item comment.
    /// </summary>
    public class ItemComment
    {
        public long OrderItemId { get; set; }
        public string Comments { get; set; }
        public string Reason { get; set; }
    }

}
