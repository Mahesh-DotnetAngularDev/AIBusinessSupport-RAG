using AIBusinessSupport.API.Data;
using AIBusinessSupport.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AIBusinessSupport.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrderDto>> GetAllAsync()
        {
            return await BuildOrderQuery().ToListAsync();
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            return await BuildOrderQuery().FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<List<OrderDto>> GetByCustomerIdAsync(int customerId)
        {
            return await BuildOrderQuery().Where(order => order.CustomerId == customerId).ToListAsync();
        }

        public async Task<List<OrderDto>> GetByStatusAsync(string status)
        {
            return await BuildOrderQuery().Where(order => order.Status.ToLower() == status.ToLower()).ToListAsync();
        }
        
        private IQueryable<OrderDto> BuildOrderQuery()
    {
        return _db.Orders
            .AsNoTracking()
            .Select(order => new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.Name,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,

                Items = order.OrderItems
                    .Select(item => new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    })
                    .ToList()
            });
    }
    }
}