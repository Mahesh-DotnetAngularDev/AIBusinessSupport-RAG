using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIBusinessSupport.API.DTOs;

namespace AIBusinessSupport.API.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();

    Task<CustomerDto?> GetByIdAsync(int id);

    Task<CustomerDto?> GetByEmailAsync(string email);
    Task<List<CustomerDto>> SearchByNameAsync(string name);
    }
}