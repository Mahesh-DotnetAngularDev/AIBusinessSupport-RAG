using AIBusinessSupport.Api.DTOs;

namespace AIBusinessSupport.Api.Services;

public interface ISupportTicketService
{
    Task<List<SupportTicketDto>> GetAllAsync();

    Task<SupportTicketDto?> GetByIdAsync(int id);

    Task<List<SupportTicketDto>> GetByCustomerIdAsync(int customerId);

    Task<List<SupportTicketDto>> GetByStatusAsync(string status);

    Task<List<SupportTicketDto>> GetByPriorityAsync(string priority);
}