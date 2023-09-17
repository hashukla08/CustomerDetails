using System.ComponentModel.DataAnnotations;

namespace CustomerDetails.DataAccess.Models
{
    public class CustomerRequest
    {
        public string CustomerName { get; set; } = string.Empty;

        public string DateOfBirth { get; set; }
    }
}
