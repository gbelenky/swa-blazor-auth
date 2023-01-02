using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Data;

public static class ProductData 
{   
    public static Task<IEnumerable<Product>> GetProducts()
    {
        List<Product>products = new List<Product>
        {
            new Product
            {
                Id = 10,
                Name = "Strawberries",
                Description = "16oz package of fresh organic strawberries",
                Quantity = 1
            },
            new Product
            {
                Id = 20,
                Name = "Sliced bread",
                Description = "Loaf of fresh sliced wheat bread",
                Quantity = 1
            },
            new Product
            {
                Id = 30,
                Name = "Apples",
                Description = "Bag of 7 fresh McIntosh apples",
                Quantity = 1
            }
        };
 
        return Task.FromResult(products.AsEnumerable());
    }
}
