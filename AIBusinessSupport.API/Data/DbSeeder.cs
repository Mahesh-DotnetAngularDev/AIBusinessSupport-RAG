using AIBusinessSupport.Api.Models;
using AIBusinessSupport.API.Data;
using AIBusinessSupport.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIBusinessSupport.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Customers.CountAsync() >= 25)
        {
            return;
        }

        var customers = new List<Customer>
        {
            new() { Name = "Arjun Mehta", Email = "arjun.mehta@example.com", Phone = "9876543214" },
            new() { Name = "Neha Singh", Email = "neha.singh@example.com", Phone = "9876543215" },
            new() { Name = "Vikram Rao", Email = "vikram.rao@example.com", Phone = "9876543216" },
            new() { Name = "Ananya Roy", Email = "ananya.roy@example.com", Phone = "9876543217" },
            new() { Name = "Rohan Gupta", Email = "rohan.gupta@example.com", Phone = "9876543218" },
            new() { Name = "Kavya Nair", Email = "kavya.nair@example.com", Phone = "9876543219" },
            new() { Name = "Siddharth Jain", Email = "siddharth.jain@example.com", Phone = "9876543220" },
            new() { Name = "Pooja Mishra", Email = "pooja.mishra@example.com", Phone = "9876543221" },
            new() { Name = "Aditya Verma", Email = "aditya.verma@example.com", Phone = "9876543222" },
            new() { Name = "Isha Kapoor", Email = "isha.kapoor@example.com", Phone = "9876543223" },
            new() { Name = "Manish Das", Email = "manish.das@example.com", Phone = "9876543224" },
            new() { Name = "Riya Sen", Email = "riya.sen@example.com", Phone = "9876543225" },
            new() { Name = "Karan Malhotra", Email = "karan.malhotra@example.com", Phone = "9876543226" },
            new() { Name = "Meera Joshi", Email = "meera.joshi@example.com", Phone = "9876543227" },
            new() { Name = "Nikhil Sahu", Email = "nikhil.sahu@example.com", Phone = "9876543228" },
            new() { Name = "Tanvi Agarwal", Email = "tanvi.agarwal@example.com", Phone = "9876543229" },
            new() { Name = "Rahul Iyer", Email = "rahul.iyer@example.com", Phone = "9876543230" },
            new() { Name = "Simran Kaur", Email = "simran.kaur@example.com", Phone = "9876543231" },
            new() { Name = "Varun Bhat", Email = "varun.bhat@example.com", Phone = "9876543232" },
            new() { Name = "Divya Reddy", Email = "divya.reddy@example.com", Phone = "9876543233" },
            new() { Name = "Aakash Patnaik", Email = "aakash.patnaik@example.com", Phone = "9876543234" }
        };

        var products = new List<Product>
        {
            new()
            {
                Name = "Business Laptop 14",
                Description = "Lightweight business laptop with 16GB RAM and 512GB SSD.",
                Price = 72000,
                StockQuantity = 15
            },
            new()
            {
                Name = "USB-C Docking Station",
                Description = "Multi-port USB-C docking station for laptops and monitors.",
                Price = 6499,
                StockQuantity = 25
            },
            new()
            {
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with adjustable DPI.",
                Price = 1499,
                StockQuantity = 50
            },
            new()
            {
                Name = "Webcam Full HD",
                Description = "1080p webcam suitable for meetings and online presentations.",
                Price = 3499,
                StockQuantity = 30
            },
            new()
            {
                Name = "Office Chair",
                Description = "Ergonomic office chair with adjustable lumbar support.",
                Price = 12500,
                StockQuantity = 10
            },
            new()
            {
                Name = "External SSD 1TB",
                Description = "Portable 1TB SSD with high-speed USB-C connectivity.",
                Price = 8999,
                StockQuantity = 18
            },
            new()
            {
                Name = "Bluetooth Speaker",
                Description = "Portable Bluetooth speaker with 20-hour battery life.",
                Price = 2999,
                StockQuantity = 28
            },
            new()
            {
                Name = "Laptop Stand",
                Description = "Adjustable aluminum laptop stand for desk setups.",
                Price = 2199,
                StockQuantity = 40
            },
            new()
            {
                Name = "USB-C Cable",
                Description = "High-speed USB-C charging and data cable.",
                Price = 799,
                StockQuantity = 100
            },
            new()
            {
                Name = "Mechanical Keyboard Pro",
                Description = "Premium mechanical keyboard with RGB lighting.",
                Price = 6999,
                StockQuantity = 22
            },
            new()
            {
                Name = "27 Inch QHD Monitor",
                Description = "27-inch QHD monitor with 165Hz refresh rate.",
                Price = 28999,
                StockQuantity = 14
            },
            new()
            {
                Name = "Noise Cancelling Earbuds",
                Description = "Wireless earbuds with active noise cancellation.",
                Price = 5999,
                StockQuantity = 32
            },
            new()
            {
                Name = "Power Bank 20000mAh",
                Description = "High-capacity power bank with fast charging support.",
                Price = 2499,
                StockQuantity = 45
            },
                new()
            {
                Name = "WiFi 6 Router",
                Description = "Dual-band WiFi 6 router for high-speed home and office networks.",
                Price = 5499,
                StockQuantity = 20
            },
            new()
            {
                Name = "Tablet 10 Inch",
                Description = "10-inch tablet with 8GB RAM and 128GB storage.",
                Price = 21999,
                StockQuantity = 16
            },
            new()
            {
                Name = "Smartphone Pro",
                Description = "Premium smartphone with 256GB storage and advanced camera.",
                Price = 54999,
                StockQuantity = 12
            }
        };

        db.Customers.AddRange(customers);
        db.Products.AddRange(products);

        await db.SaveChangesAsync();

        var allCustomers = await db.Customers
            .OrderBy(x => x.Id)
            .ToListAsync();

        var allProducts = await db.Products
            .OrderBy(x => x.Id)
            .ToListAsync();

        var orders = new List<Order>
        {
            CreateOrder(allCustomers[4], allProducts[0], 1, "Completed"),
            CreateOrder(allCustomers[5], allProducts[2], 2, "Completed"),
            CreateOrder(allCustomers[6], allProducts[10], 1, "Processing"),
            CreateOrder(allCustomers[7], allProducts[11], 2, "Completed"),
            CreateOrder(allCustomers[8], allProducts[5], 1, "Pending"),
            CreateOrder(allCustomers[9], allProducts[14], 1, "Completed"),
            CreateOrder(allCustomers[10], allProducts[4], 1, "Processing"),
            CreateOrder(allCustomers[11], allProducts[7], 2, "Completed"),
            CreateOrder(allCustomers[12], allProducts[15], 1, "Pending"),
            CreateOrder(allCustomers[13], allProducts[6], 1, "Completed"),
            CreateOrder(allCustomers[14], allProducts[9], 1, "Processing"),
            CreateOrder(allCustomers[15], allProducts[12], 2, "Completed"),
            CreateOrder(allCustomers[16], allProducts[13], 1, "Completed"),
            CreateOrder(allCustomers[17], allProducts[3], 1, "Pending"),
            CreateOrder(allCustomers[18], allProducts[1], 2, "Completed"),
            CreateOrder(allCustomers[19], allProducts[8], 3, "Completed"),
            CreateOrder(allCustomers[20], allProducts[10], 1, "Processing")
        };

        db.Orders.AddRange(orders);

        var tickets = new List<SupportTicket>
        {
            CreateTicket(allCustomers[4], "Laptop battery issue",
                "The laptop battery is draining faster than expected.", "Open", "High"),

            CreateTicket(allCustomers[5], "Mouse connectivity problem",
                "The wireless mouse disconnects intermittently.", "In Progress", "Medium"),

            CreateTicket(allCustomers[6], "Monitor display issue",
                "The monitor occasionally shows a black screen.", "Open", "High"),

            CreateTicket(allCustomers[7], "Order tracking request",
                "Customer wants an update on the shipment status.", "Closed", "Low"),

            CreateTicket(allCustomers[8], "SSD not detected",
                "External SSD is not being detected by the laptop.", "Open", "High"),

            CreateTicket(allCustomers[9], "Router configuration help",
                "Customer needs assistance configuring the WiFi router.", "In Progress", "Medium"),

            CreateTicket(allCustomers[10], "Chair assembly support",
                "Customer needs help assembling the office chair.", "Closed", "Low"),

            CreateTicket(allCustomers[11], "Keyboard key issue",
                "One of the keyboard keys is not responding properly.", "Open", "Medium"),

            CreateTicket(allCustomers[12], "Payment failed",
                "Payment failed but the amount appears to have been deducted.", "Open", "High"),

            CreateTicket(allCustomers[13], "Earbuds charging problem",
                "The earbuds are not charging correctly.", "In Progress", "High"),

            CreateTicket(allCustomers[14], "Delivery address change",
                "Customer requested a change to the delivery address.", "Closed", "Medium"),

            CreateTicket(allCustomers[15], "Webcam microphone issue",
                "The webcam microphone is not being detected.", "Open", "Medium"),

            CreateTicket(allCustomers[16], "Power bank not charging",
                "Power bank is not charging after being connected overnight.", "Open", "High"),

            CreateTicket(allCustomers[17], "Tablet setup assistance",
                "Customer needs help setting up the new tablet.", "Closed", "Low"),

            CreateTicket(allCustomers[18], "Smartphone overheating",
                "Customer reports that the smartphone becomes hot during normal usage.", "Open", "High"),

            CreateTicket(allCustomers[19], "USB cable replacement",
                "Customer received a damaged USB-C cable.", "In Progress", "Medium"),

            CreateTicket(allCustomers[20], "Bluetooth speaker pairing",
                "Speaker is not pairing with the customer's phone.", "Open", "Medium")
        };

        db.SupportTickets.AddRange(tickets);

        await db.SaveChangesAsync();
    }

    private static Order CreateOrder(
        Customer customer,
        Product product,
        int quantity,
        string status)
    {
        var total = product.Price * quantity;

        return new Order
        {
            CustomerId = customer.Id,
            Status = status,
            TotalAmount = total,
            OrderItems =
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price
                }
            }
        };
    }

    private static SupportTicket CreateTicket(
        Customer customer,
        string subject,
        string description,
        string status,
        string priority)
    {
        return new SupportTicket
        {
            CustomerId = customer.Id,
            Subject = subject,
            Description = description,
            Status = status,
            Priority = priority
        };
    }
}