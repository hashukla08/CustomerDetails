using System.ComponentModel.DataAnnotations;

namespace CustomerDetails.API.DataAccess.DTO
{
	public class CreateCustomerRequest
	{
		[Required]
		public string CustomerName { get; set; } = string.Empty;
		
		public string DateOfBirth { get; set; }
	}
}
