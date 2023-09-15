using CustomerDetails.API.DataAccess.Models;

namespace CustomerDetails.API.Services
{
	public interface IBaseService
	{
		APIResponse response { get; set; }

		Task<string> SendAsync(APIRequest requests);
	}
}
