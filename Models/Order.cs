﻿using System;
using System.Collections.Generic;

namespace ZenDeskTicketProcessJob.Models
{
    /// <summary>
    /// Order.
    /// </summary>
    public class Order
    {
        public long OrderId { get; set; }
        public long OrderChangeRequestId { get; set; }
        public string NHMemberId { get; set; }
        public string ItemDetails { get; set; }
        public string CarrierName { get; set; }

        public long InsuranceCarrierId { get; set; }
        public string ItemComments { get; set; }
        public string UserName { get; set; }
        public string SubmittedBy { get; set; }
        public string RequestedDate { get; set; }
        public string RequestType { get; set; }
        public string Status { get; set; }
        public string AdminComments { get; set; }
        public decimal TotalPrice { get; set; }
        public bool? IsProcessed { get; set; }
        public string TicketId { get; set; }
    }

    /// <summary>
    /// Item detail.
    /// </summary>
    public class ItemDetail
    {
        public string ItemName { get; set; }
        public long Quantity { get; set; }
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