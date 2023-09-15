using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace CustomerDetails.API.Services
{
	public class BaseService : IBaseService
	{
		public APIResponse response { get; set; }
		public IHttpClientFactory httpClient { get; set; }

		public BaseService(IHttpClientFactory httpClient)
		{
			this.response = new();
			this.httpClient = httpClient;
		}
		public async Task<string> SendAsync(APIRequest request)
		{
			try
			{
				var client = httpClient.CreateClient("CustomerClient");
				HttpRequestMessage message = new HttpRequestMessage();
				//message.Headers.Add("Accept", "application/json");
				message.RequestUri = new Uri(request.requestURL);
				if (request.requestData != null)
				{
					message.Content = new StringContent(JsonConvert.SerializeObject(request.requestData), Encoding.UTF8, "application/json");
				}
				message.Method = request.requestType switch

				{
					SD.APIType.POST => HttpMethod.Post,
					SD.APIType.PUT => HttpMethod.Put,
					SD.APIType.DELETE => HttpMethod.Delete,
					SD.APIType.PATCH => HttpMethod.Patch,
					_ => HttpMethod.Get

				};
				HttpResponseMessage responseMessage = await client.SendAsync(message);
				var apiContent = await responseMessage.Content.ReadAsStringAsync();

				return apiContent;
			}
			catch (Exception e)
			{
				var dto = new APIResponse
				{
					IsSuccess = false,
					ErrorMessage = new List<string> { Convert.ToString(e.Message) }
				};

				var res = JsonConvert.SerializeObject(dto);
				var apiResponse = JsonConvert.DeserializeObject<string>(res);
				return string.Empty;
			}
		}
	}

}
