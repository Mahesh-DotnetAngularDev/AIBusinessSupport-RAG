using AIBusinessSupport.Api.Models;

namespace AIBusinessSupport.API.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
