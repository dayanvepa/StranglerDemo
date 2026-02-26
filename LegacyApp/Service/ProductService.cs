using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegacyApp.Models;

namespace LegacyApp.Service
{
    public interface IProductService
    {
        List<ProductDto> GetProducts();
    }
    public class ProductService : IProductService
    {
        public List<ProductDto> GetProducts()
        {
            return new List<ProductDto>
            {
                new ProductDto
                {
                    Id = 1,
                    Name = "Laptop Pro 15",
                    Category = "Electrónica",
                    Price = 1200.00m,
                    Stock = 15
                },
                new ProductDto
                {
                    Id = 2,
                    Name = "Mouse Ergonómico",
                    Category = "Accesorios",
                    Price = 45.99m,
                    Stock = 50
                },
                new ProductDto
                {
                    Id = 3,
                    Name = "Teclado Mecánico RGB",
                    Category = "Accesorios",
                    Price = 129.90m,
                    Stock = 20
                }
            };
        }
    }
    
}