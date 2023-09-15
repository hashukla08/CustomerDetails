using System.ComponentModel.DataAnnotations;

namespace CustomerDetails.API.DataAccess.DTO
{
	public class CreateCustomerDTO
	{
		[Required]
		public string CustomerName { get; set; }=string.Empty;
		[Required]
		public DateOnly DateOfBirth { get; set; }
		
	}
}
