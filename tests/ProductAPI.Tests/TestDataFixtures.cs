using ProductAPI.Models;

namespace ProductAPI.Tests
{
    public static class TestDataFixtures
    {
        public static class ValidProducts
        {
            public static Product Laptop => new()
            {
                Name = "Laptop",
                Description = "High-performance business laptop",
                Price = 999.99m,
                Category = "Electronics",
                Stock = 15
            };

            public static Product Mouse => new()
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                Price = 29.99m,
                Category = "Accessories",
                Stock = 50
            };

            public static Product Keyboard => new()
            {
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard",
                Price = 149.99m,
                Category = "Accessories",
                Stock = 25
            };

            public static Product Monitor => new()
            {
                Name = "4K Monitor",
                Description = "27-inch 4K Ultra HD monitor",
                Price = 599.99m,
                Category = "Electronics",
                Stock = 8
            };

            public static Product OutOfStock => new()
            {
                Name = "Limited Edition Item",
                Description = "Exclusive limited edition product",
                Price = 2999.99m,
                Category = "Premium",
                Stock = 0
            };
        }

        public static class InvalidProducts
        {
            public static Product NegativePrice => new()
            {
                Name = "Invalid Price Product",
                Price = -50m,
                Stock = 10
            };

            public static Product ZeroPrice => new()
            {
                Name = "Zero Price Product",
                Price = 0m,
                Stock = 10
            };

            public static Product NegativeStock => new()
            {
                Name = "Negative Stock Product",
                Price = 99.99m,
                Stock = -5
            };

            public static Product EmptyName => new()
            {
                Name = string.Empty,
                Price = 50m,
                Stock = 10
            };

            public static Product VeryLongName => new()
            {
                Name = new string('A', 201),
                Price = 50m,
                Stock = 10
            };
        }

        public static class ProductRequests
        {
            public static Dictionary<string, object> ValidProductJson => new()
            {
                { "name", "Test Product" },
                { "description", "Test Description" },
                { "price", 99.99 },
                { "category", "Test Category" },
                { "stock", 20 }
            };

            public static Dictionary<string, object> MinimalValidProductJson => new()
            {
                { "name", "Minimal Product" },
                { "price", 0.01 },
                { "stock", 0 }
            };

            public static Dictionary<string, object> InvalidPriceJson => new()
            {
                { "name", "Invalid Price" },
                { "price", -10 },
                { "stock", 5 }
            };

            public static Dictionary<string, object> InvalidStockJson => new()
            {
                { "name", "Invalid Stock" },
                { "price", 99.99 },
                { "stock", -5 }
            };

            public static Dictionary<string, object> MissingNameJson => new()
            {
                { "price", 99.99 },
                { "stock", 10 }
            };

            public static Dictionary<string, object> MissingPriceJson => new()
            {
                { "name", "Test Product" },
                { "stock", 10 }
            };

            public static Dictionary<string, object> MissingStockJson => new()
            {
                { "name", "Test Product" },
                { "price", 99.99 }
            };
        }

        public static class BoundaryValues
        {
            public static readonly decimal MinValidPrice = 0.01m;
            public static readonly decimal MaxValidPrice = decimal.MaxValue;
            public static readonly int MinValidStock = 0;
            public static readonly int MaxValidStock = int.MaxValue;
            public static readonly decimal InvalidPriceBelow = -0.01m;
            public static readonly int InvalidStockBelow = -1;
        }
    }
}
