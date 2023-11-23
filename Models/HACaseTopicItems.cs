using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenDeskTicketProcessJob.Models
{
    namespace ZenDeskTicketProcessJob.Models
    {
        public class Issue
        {
            public string IssueName { get; set; }
            public string IssueId { get; set; }
        }

        public class ItemInfo
        {
            public string ItemId { get; set; }
            public object Item { get; set; }
            public int TotalQuantity { get; set; }
            public decimal Price { get; set; }
            public List<Issue> Issue { get; set; }
            public int ImpactedQuantity { get; set; }
            public decimal ImpactedPrice { get; set; }
            public string ItemData { get; set; }
            public string ItemCode { get; set; }
        }

        public class Order
        {
            public long OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class Root
        {
            public string Additinalinfro { get; set; }
            public List<ItemInfo> ItemInfo { get; set; }
            public Order Order { get; set; }
        }
    }

}
