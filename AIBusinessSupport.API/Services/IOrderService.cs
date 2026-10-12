using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIBusinessSupport.API.DTOs;

namespace AIBusinessSupport.API.Services
{
    public interface IOrderService
    {
    Task<List<OrderDto>> GetAllAsync();

    Task<OrderDto?> GetByIdAsync(int id);

    Task<List<OrderDto>> GetByCustomerIdAsync(int customerId);

    Task<List<OrderDto>> GetByStatusAsync(string status);
    }
}