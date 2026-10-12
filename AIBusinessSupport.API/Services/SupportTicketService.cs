using AIBusinessSupport.Api.Data;
using AIBusinessSupport.Api.DTOs;
using AIBusinessSupport.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AIBusinessSupport.Api.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly AppDbContext _db;

    public SupportTicketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SupportTicketDto>> GetAllAsync()
    {
        return await BuildTicketQuery()
            .ToListAsync();
    }

    public async Task<SupportTicketDto?> GetByIdAsync(int id)
    {
        return await BuildTicketQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<SupportTicketDto>> GetByCustomerIdAsync(int customerId)
    {
        return await BuildTicketQuery()
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<List<SupportTicketDto>> GetByStatusAsync(string status)
    {
        return await BuildTicketQuery()
            .Where(x => x.Status.ToLower() == status.ToLower())
            .ToListAsync();
    }

    public async Task<List<SupportTicketDto>> GetByPriorityAsync(string priority)
    {
        return await BuildTicketQuery()
            .Where(x => x.Priority.ToLower() == priority.ToLower())
            .ToListAsync();
    }

    private IQueryable<SupportTicketDto> BuildTicketQuery()
    {
        return _db.SupportTickets
            .AsNoTracking()
            .Select(ticket => new SupportTicketDto
            {
                Id = ticket.Id,
                CustomerId = ticket.CustomerId,
                CustomerName = ticket.Customer.Name,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            });
    }
}