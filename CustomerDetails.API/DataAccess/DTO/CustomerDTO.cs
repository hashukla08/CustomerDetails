using System.ComponentModel.DataAnnotations;

namespace CustomerDetails.API.DataAccess.DTO
{
	public class CustomerDTO
	{
		public Guid CustomerId { get; set; }
		public string CustomerName { get; set; } = string.Empty;

		public DateOnly DateOfBirth { get; set; }
		public string ProfileImage { get; set; }= string.Empty;
	}
}
