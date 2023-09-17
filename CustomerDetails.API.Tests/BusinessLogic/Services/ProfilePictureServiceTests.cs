using CustomerDetails.BusinessLogic.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Net.Http;

namespace CustomerDetails.API.Tests.BusinessLogic.Services
{
	public class ProfilePictureServiceTests
	{
		private readonly ProfilePictureService _service;
		private readonly Mock<IHttpClientFactory> _mockClientFactory;

		public ProfilePictureServiceTests()
		{
			var expectedResponseContent = "svg content";
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(expectedResponseContent),
			};
			var httpClientMock = new Mock<HttpClient>(); 
			httpClientMock
			.Setup(httpClient => httpClient.SendAsync(
				It.IsAny<HttpRequestMessage>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(httpResponseMessage);
			_mockClientFactory = new Mock<IHttpClientFactory>();
			_mockClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

			var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(
				new Dictionary<string, string>
				{
					{"UIAvatar:BaseUrl", "http://www.local.com/"}
				})
			.Build();
			_service = new ProfilePictureService(_mockClientFactory.Object, configuration);
		}

		[Fact]
		public async Task GetBase64EncodedSvgProfilePictureAsync_Pass()
		{
			//Arrange
			string name = "test name";

			//Act
			var response = await _service.GetBase64EncodedSvgProfilePictureAsync(name);

			//Assert
			response.Should().NotBeNull();
		}
	}
}
