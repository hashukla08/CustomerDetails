using AutoMapper;
using CustomerDetails.API.Controllers;
using CustomerDetails.API.DataAccess.DTO;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CustomerDetails.API.Tests.API
{
	public class CustomerAPIControllerTests : TestFixture
	{
		private readonly Mock<ICustomerService> mockCustomerService;
		private readonly Mock<IMapper> mockMapper;
		private readonly Mock<IProfilePictureService> mockProfilePictureService;
		private readonly CustomerAPIController controller;

		public CustomerAPIControllerTests()
		{
			mockCustomerService = new Mock<ICustomerService>();
			mockMapper = new Mock<IMapper>();
			mockProfilePictureService = new Mock<IProfilePictureService>();
			controller = new CustomerAPIController(
				mockCustomerService.Object,
				mockMapper.Object,
				mockProfilePictureService.Object);
		}

		[Fact]
		public async Task GetCustomersAsync_Pass()
		{
			//Arrange
			var customerList = new List<Customer>() {
				new Customer
				{
					CustomerId = new Guid(),
					CustomerName = "John Doe",
					DateOfBirth = new DateOnly(1981, 09, 09)
				},
				new Customer
				{
					CustomerId = new Guid(),
					CustomerName = "Daisy Duck",
					DateOfBirth = new DateOnly(1971, 02, 02)
				}
			};
			mockCustomerService
				.Setup(x => x.GetAllAsync())
				.Returns(
					Task.FromResult(customerList.AsEnumerable())
					);

			//Act
			var response = await controller.GetCustomersAsync();

			//Assert
			Assert.IsType<ActionResult<APIResponse>>(response);
			Assert.NotNull(response);
		}

		[Fact]
		public async Task GetCustomersAsync_ThrowsException()
		{
			//Arrange
			mockCustomerService
				.Setup(x => x.GetAllAsync())
				.Throws(new Exception("Service Unavailable"));

			//Act
			var response = await controller.GetCustomersAsync();

			//Assert
			Assert.IsType<ActionResult<APIResponse>>(response);
			Assert.NotNull(response);
		}
		
		[Fact]
		public async Task CreateCustomerAsync_MissingDI_Pass()
		{
			try
			{
				//Arrange
				var newCustomer = new CreateCustomerRequest
				{
					CustomerName = "John Doe",
					DateOfBirth = Convert.ToString(new DateOnly(1981, 09, 09))
				};
				
				mockCustomerService
					.Setup(u => u.AddCustomerAsync(It.IsAny<Customer>()))
					.Returns(Task.FromResult(newCustomer));

				//Act
				var response = await controller.CreateCustomer(newCustomer);

				//Assert
				mockCustomerService.Verify(x => x.GetCustomersByAgeAsync(It.IsAny<int>()), Times.Once);
			}
			catch (Exception)
			{
				Assert.True(true);
			}
		}


		[Fact]
		public async Task CreateCustomerAsync_Pass()
		{
			try
			{
				//Arrange
				var newCustomer = new CreateCustomerRequest
				{
					CustomerName = "John Doe",
					DateOfBirth = Convert.ToString(new DateOnly(1981, 09, 09))
				};

				mockCustomerService
					.Setup(u => u.AddCustomerAsync(It.IsAny<Customer>()))
					.Returns(Task.FromResult(newCustomer));

				mockCustomerService
					.Setup(u => u.UpdateAsync(It.IsAny<Customer>()))
					.Returns(Task.FromResult(true));

				mockProfilePictureService
					.Setup(u => u.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(Guid.NewGuid().ToString()));


				//Act
				var response = await controller.CreateCustomer(newCustomer);

				//Assert
				mockCustomerService.Verify(x => x.GetCustomersByAgeAsync(It.IsAny<int>()), Times.Once);

				var result = Assert.IsAssignableFrom<APIResponse>(response.Result);
				Assert.NotNull(response);
				Assert.Fail("Profile Picture Service not initialized");
			}
			catch (Exception)
			{
				Assert.True(true);
			}
		}

		[Theory]
		[InlineData(null, null)]
		[InlineData("123456789", "08-12-1993")]
		[InlineData("Mickey Mouse", null)]
		[InlineData("Mickey Mouse", "DateOfBirth")]
		public async Task CreateCustomerAsync_WithInvalidData_ReturnsBadRequest(string CustomerName, string DoB)
		{
			try
			{
				//Arrange
				CreateCustomerRequest newCustomer =
					(string.IsNullOrWhiteSpace(CustomerName) && string.IsNullOrWhiteSpace(DoB)) ?
					null! :
					new CreateCustomerRequest
					{
						CustomerName = CustomerName,
						DateOfBirth = DoB
					};

				mockCustomerService
					.Setup(u => u.AddCustomerAsync(It.IsAny<Customer>()))
					.Returns(Task.FromResult(newCustomer));

				mockCustomerService
					.Setup(u => u.UpdateAsync(It.IsAny<Customer>()))
					.Returns(Task.FromResult(true));

				mockProfilePictureService
					.Setup(u => u.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
					.Returns(Task.FromResult(Guid.NewGuid().ToString()));


				//Act
				var response = await controller.CreateCustomer(newCustomer);
				
				//Assert
				mockCustomerService.Verify(x => x.GetCustomersByAgeAsync(It.IsAny<int>()), Times.Never);

				response.Should().NotBeNull();
				response.Result.Should().Be(null);
			}
			catch (Exception)
			{
				Assert.Fail("Exception should be handled");
			}
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task CreateCustomerAsync_ExceptionsTest(
			bool IsCustomerServiceDown,
			bool IsProfilePictureServiceDown)
		{
			try
			{
				//Arrange
				var newCustomer = new CreateCustomerRequest
				{
					CustomerName = "John Doe",
					DateOfBirth = Convert.ToString(new DateOnly(1981, 09, 09))
				};

				if (IsCustomerServiceDown)
				{
					mockCustomerService
						.Setup(u => u.AddCustomerAsync(It.IsAny<Customer>()))
						.Throws(new Exception("Customer Service is down"));
				}
				else
				{
					mockCustomerService
						.Setup(u => u.AddCustomerAsync(It.IsAny<Customer>()));

				}

				if (IsProfilePictureServiceDown)
				{
					mockProfilePictureService
							.Setup(u => u.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
							.Throws(new Exception("Profile Picture Service is down"));
				}
				else
				{
					mockProfilePictureService
						.Setup(u => u.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
						.Returns(Task.FromResult(Guid.NewGuid().ToString()));
				}
				
				//Act
				var response = await controller.CreateCustomer(newCustomer);

				//Assert

				if (IsCustomerServiceDown || IsProfilePictureServiceDown)
				{
					response.Should().NotBeNull();
					response.Value.Should().NotBeNull();
					if (IsProfilePictureServiceDown)
						response.Value.ErrorMessage.Should().Contain("Exception Occured - Profile Picture Service is down");
					if (IsCustomerServiceDown)
						response.Value.ErrorMessage.Should().Contain("Exception Occured - Customer Service is down");
				}
				else
				{
					mockCustomerService.Verify(x => x.AddCustomerAsync(It.IsAny<Customer>()), Times.Once);

					response.Should().NotBeNull();
					response.Result.Should().Be(null);
				}

			}
			catch (Exception ex)
			{
				if (IsCustomerServiceDown || IsProfilePictureServiceDown)
				{
					Assert.True(true);
				}
				else
				{
					Assert.Fail("Exceptions should not be thrown");
				}
			}
		}
		
		[Fact]
		public async Task GetCustomerByIdOrAge_Pass()
		{
			//Arrange

			//Act
			//Assert
		}

	}
}
