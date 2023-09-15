using CustomerDetails.API.DataAccess.Models;
using System.Net.Http;

namespace CustomerDetails.API.Services
{
	public class ProfilePictureService :BaseService, IProfilePictureService
	{
		private readonly IHttpClientFactory _httpClient;
		private readonly string? serviceUrl;
		public ProfilePictureService(IHttpClientFactory httpClient, IConfiguration configuration) : base(httpClient)
		{
			_httpClient = httpClient;
			serviceUrl = configuration.GetValue<string>("ServiceURL:UIAvatar");

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
