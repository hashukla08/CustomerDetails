using System.ComponentModel.DataAnnotations;

namespace CustomerDetails.API.DataAccess.DTO
{
	public class UpdateCustomerDTO
	{
		[Required]
		public Guid CustomerId { get; set; }
		[Required]
		public string CustomerName { get; set; } = string.Empty;
		[Required]
		public DateOnly DateOfBirth { get; set; }
		public string ProfileImage { get; set; } = string.Empty;

	}

}
