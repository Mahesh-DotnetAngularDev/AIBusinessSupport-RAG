using AIBusinessSupport.API.Data;
using AIBusinessSupport.API.DTOs;
using AIBusinessSupport.API.Services;
using Microsoft.EntityFrameworkCore;

namespace AIBusinessSupport.API.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _db.Customers
            .AsNoTracking()
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(x => x.Email == email)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone
            })
            .FirstOrDefaultAsync();
    }

    public Task<List<CustomerDto>> SearchByNameAsync(string name)
    {
        return _db.Customers
            .AsNoTracking()
            .Where(x => x.Name.Contains(name))
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Phone = x.Phone
            })
            .ToListAsync();
    }
}