using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using Microsoft.Extensions.Configuration;

namespace CustomerDetails.API.Services
{
	public class ProfilePictureService : BaseService, IProfilePictureService
	{
		private readonly string? serviceUrl;

		public ProfilePictureService(
			IHttpClientFactory httpClient,
			IConfiguration configuration)
			: base(httpClient)
		{
			serviceUrl = configuration.GetValue<string>("UIAvatar:BaseUrl");
		}

		public async Task<string> GetProfilePictureAsync(string CustomerName)
		{
			var builder = new UriBuilder(serviceUrl);
			builder.Path = "/api/";
			builder.Query = $"name={Uri.EscapeDataString(CustomerName)}&format={Uri.EscapeDataString("svg")}";
			return await SendAsync(new APIRequest()
			{
				requestType = SD.APIType.GET,
				requestURL = builder.ToString()
			});
		}
	}
}
