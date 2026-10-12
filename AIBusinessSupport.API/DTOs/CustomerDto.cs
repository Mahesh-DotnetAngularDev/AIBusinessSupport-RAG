using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIBusinessSupport.API.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}