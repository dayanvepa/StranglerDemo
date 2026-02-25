using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegacyApp.Models
{
    public record ProductDto(
        int Id,
        string Name,
        string Category,
        decimal Price,
        int Stock
    );
}