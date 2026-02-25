using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewOrderService.DTOs
{
    public record OrderDto(
        int Id,
        string Customer,
        DateTime Date,
        decimal Total,
        string Status,
        int ItemsCount
    );
}