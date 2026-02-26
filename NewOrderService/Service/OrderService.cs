using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewOrderService.DTOs;

namespace NewOrderService.Service
{
    public interface IOrderService
    {
         List<OrderDto> GetOrders();
    }
    public class OrderService : IOrderService
    {
        public List<OrderDto> GetOrders()
        {
            return new List<OrderDto>
            {
                new OrderDto(101, "Juan Pérez",  DateTime.Now.AddDays(-4), 150.50m, "Completado", 3),
                new OrderDto(102, "María García", DateTime.Now.AddDays(-3),  85.00m, "Pendiente",   1),
                new OrderDto(103, "Carlos Ruiz", DateTime.Now, 2400.00m, "Completado",   2)
            };
        }
    }
}