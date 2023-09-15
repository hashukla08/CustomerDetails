namespace CustomerDetails.API.Services
{
	public interface IProfilePictureService
	{
		Task<string> GetProfilePictureAsync(string CustomerName);
	}
}
