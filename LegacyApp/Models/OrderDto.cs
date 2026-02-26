using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LegacyApp.Models
{
    [XmlType("Order")] 
    public class OrderDto
    {
        public OrderDto() { }

        public int Id { get; set; }
        public string Customer { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
    }
}