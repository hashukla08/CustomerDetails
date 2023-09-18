using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using Ganss.Xss;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace CustomerDetails.BusinessLogic.Services
{
	public class ProfilePictureService : IProfilePictureService
	{
		private readonly string? serviceUrl;
		private readonly HttpClient _httpClient;

		public ProfilePictureService(
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration)
		{
			serviceUrl = configuration.GetValue<string>("UIAvatar:BaseUrl");
			_httpClient = httpClientFactory.CreateClient("CustomerClient");
		}

		public async Task<string> GetBase64EncodedSvgProfilePictureAsync(string customerName)
		{
			var svgData = await GetProfilePictureAsync(customerName);

			var sanitizedSVG = SanitizeSVG(svgData);

			byte[] dataBytes = Encoding.UTF8.GetBytes(sanitizedSVG);
			string profilePicture = Convert.ToBase64String(dataBytes);

			return profilePicture;
		}

		private async Task<string> GetProfilePictureAsync(string CustomerName)
		{
			var builder = new UriBuilder(serviceUrl);
			builder.Path = "/api/";
			builder.Query = $"name={Uri.EscapeDataString(CustomerName)}&format={Uri.EscapeDataString("svg")}";
			return await SendAsync(new APIRequest()
			{
				requestType = Constants.APIType.GET,
				requestURL = builder.ToString()
			});
		}
		private async Task<string> SendAsync(APIRequest request)
		{
			try
			{
				HttpRequestMessage message = new HttpRequestMessage();

				message.RequestUri = new Uri(request.requestURL);
				if (request.requestData != null)
				{
					message.Content = new StringContent(JsonConvert.SerializeObject(request.requestData), Encoding.UTF8, "application/json");
				}
				message.Method = request.requestType switch
				{
					Constants.APIType.POST => HttpMethod.Post,
					Constants.APIType.PUT => HttpMethod.Put,
					Constants.APIType.DELETE => HttpMethod.Delete,
					Constants.APIType.PATCH => HttpMethod.Patch,
					_ => HttpMethod.Get
				};

				HttpResponseMessage responseMessage = await _httpClient.SendAsync(message);
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

		private string SanitizeSVG(string svgContent)
		{
			var sanitizer = new HtmlSanitizer();

			// Allow SVG elements and basic attributes
			sanitizer.AllowedTags.Add("svg");
			sanitizer.AllowedTags.Add("circle");
			sanitizer.AllowedTags.Add("text");
			sanitizer.AllowedTags.Add("rect");

			// Remove potentially dangerous attributes
			RemoveEventAttributes(sanitizer);
			RemoveScriptAttributes(sanitizer);
			RemoveExternalResourceAttributes(sanitizer);

			// Sanitize the SVG content
			return sanitizer.Sanitize(svgContent);
		}

		private void RemoveEventAttributes(HtmlSanitizer sanitizer)
		{
			// Remove event attributes
			sanitizer.AllowedAttributes.Remove("onload");
			sanitizer.AllowedAttributes.Remove("onunload");
			// Add other event attributes here
		}

		private void RemoveScriptAttributes(HtmlSanitizer sanitizer)
		{
			// Remove script-related attributes
			sanitizer.AllowedAttributes.Remove("script");
			sanitizer.AllowedAttributes.Remove("xlink:href");
		}

		private void RemoveExternalResourceAttributes(HtmlSanitizer sanitizer)
		{
			// Remove attributes that reference external resources
			sanitizer.AllowedAttributes.Remove("xlink:href");
		}

		
	}
}
