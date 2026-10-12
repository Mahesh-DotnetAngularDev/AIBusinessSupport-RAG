using AIBusinessSupport.API.Services;
using System.ComponentModel;
using System.Text.Json;

public class BusinessTools
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    public BusinessTools(IOrderService orderService, ICustomerService customerService)
    {
        _orderService = orderService;
        _customerService = customerService;
    }
    [Description("Gets orders from the business database filtered by their order status.")]
    public async Task<string> GetOrderByStatus([Description("The order status to filter by, such as Pending, Processing, or Completed.")] string status)
    {
        var orders = await _orderService.GetByStatusAsync(status);
        if (orders.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                count = 0,
                status,
                message = $"No orders were found with status '{status}'."
            });
        }

        return JsonSerializer.Serialize(new
        {
            found = orders.Count > 0,
            count = orders.Count,
            status,
            orders
        });
    }

    [Description("Gets all orders placed by a specific customer from the business database.")]
    public async Task<string> GetOrdersByCustomer(
    [Description("The unique ID of the customer whose orders should be retrieved.")]
    int customerId)
    {
        var orders = await _orderService.GetByCustomerIdAsync(customerId);

        if (orders.Count == 0)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                count = 0,
                customerId,
                message = $"No orders were found for customer ID {customerId}."
            });
        }

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            found = orders.Count > 0,
            count = orders.Count,
            customerId,
            orders
        });
    }
    [Description("Searches the business database for customers by their name.")]
    public async Task<string> SearchCustomersByName(
        [Description("The name of the customer to search for.")]
        string name)
    {
        var customers = await _customerService.SearchByNameAsync(name);
        if (customers.Count == 0)
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                found = false,
                message = $"No customers were found matching '{name}'."
            });
        }

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            found = true,
            count = customers.Count,
            customers
        });
    }
}