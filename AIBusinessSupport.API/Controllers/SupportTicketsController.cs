using AIBusinessSupport.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIBusinessSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportTicketsController : ControllerBase
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketsController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _supportTicketService.GetAllAsync();

        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _supportTicketService.GetByIdAsync(id);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var tickets = await _supportTicketService.GetByCustomerIdAsync(customerId);

        return Ok(tickets);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var tickets = await _supportTicketService.GetByStatusAsync(status);

        return Ok(tickets);
    }

    [HttpGet("priority/{priority}")]
    public async Task<IActionResult> GetByPriority(string priority)
    {
        var tickets = await _supportTicketService.GetByPriorityAsync(priority);

        return Ok(tickets);
    }
}