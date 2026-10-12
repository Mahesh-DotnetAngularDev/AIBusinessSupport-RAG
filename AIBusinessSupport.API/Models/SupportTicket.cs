using AIBusinessSupport.API.Models;

namespace AIBusinessSupport.Api.Models;

public class SupportTicket
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public string Priority { get; set; } = "Medium";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
}