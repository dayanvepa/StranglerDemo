using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegacyApp.Models;
using System.IO;
using System.Xml.Serialization;

namespace LegacyApp.Service
{
    public interface IOrderService
    {
         List<OrderDto> GetOrders();
         string GetOrdersXml();
    }

    public class OrderService : IOrderService
    {
        public List<OrderDto> GetOrders()
        {
            return new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 101,
                    Customer = "Juan Pérez",
                    Date = DateTime.Now.AddDays(-4),
                    Total = 150.50m,
                    Status = "Completado",
                    ItemsCount = 3
                },
                new OrderDto
                {
                    Id = 102,
                    Customer = "María García",
                    Date = DateTime.Now.AddDays(-3),
                    Total = 85.00m,
                    Status = "Pendiente",
                    ItemsCount = 1
                },
                new OrderDto
                {
                    Id = 103,
                    Customer = "Carlos Ruiz",
                    Date = DateTime.Now,
                    Total = 2400.00m,
                    Status = "Completado",
                    ItemsCount = 2
                }
            };
        }

        //Retonar una lista de OrderDto serializados xml
        public string GetOrdersXml2(){
            var orders = GetOrders();
            var serializer = new XmlSerializer(typeof(List<OrderDto>));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, orders);
                return stringWriter.ToString();
            }
        }

        public string GetOrdersXml()
        {
            var orders = GetOrders();

            var root = new XmlRootAttribute("Orders"); 
            var serializer = new XmlSerializer(typeof(List<OrderDto>), root);

            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, orders);

            return stringWriter.ToString();
        }
    }

}