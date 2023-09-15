namespace CustomerDetails.API.DataAccess.Entities
{
	public class Customer
	{
		public Guid CustomerId { get; set; }
		public string CustomerName { get; set; } = string.Empty;
		public DateOnly DateOfBirth { get; set; } 
		public string ProfileImage { get; set; } = string.Empty;
	}
}
