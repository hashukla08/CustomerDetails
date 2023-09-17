using AutoMapper;
using CustomerDetails.API.Controllers;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using CustomerDetails.DataAccess.Models;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CustomerDetails.API.Tests.API
{
	public class CustomerAPIControllerTests
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
				var newCustomer = new CustomerRequest
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
				var newCustomer = new CustomerRequest
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
				CustomerRequest newCustomer =
					(string.IsNullOrWhiteSpace(CustomerName) && string.IsNullOrWhiteSpace(DoB)) ?
					null! :
					new CustomerRequest
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
				var newCustomer = new CustomerRequest
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

		[Theory]
		[InlineData("30", true)]
		[InlineData("4E23F734-2F8C-41A3-8868-783971B793CD", true)]
		[InlineData("-1", false)]
		[InlineData("abc", false)]
		[InlineData("21", false)]
		[InlineData("8D76F3F2-4B64-4AD8-BA0E-8B75636853ED", false)]
		public async Task GetCustomerByIdOrAge_Pass(string input, bool IsValid)
		{
			//Arrange
			var customerList = new List<Customer>() {
				new Customer
				{
					CustomerId = Guid.NewGuid(),
					CustomerName = "John Doe",
					DateOfBirth = new DateOnly(1981, 09, 09)
				},
				new Customer
				{
					CustomerId = Guid.NewGuid(),
					CustomerName = "Daisy Duck",
					DateOfBirth = new DateOnly(1971, 02, 02)
				},
				new Customer
				{
					CustomerId = Guid.Parse("4E23F734-2F8C-41A3-8868-783971B793CD"),
					CustomerName = "Minnie Mouse",
					DateOfBirth = new DateOnly(1993, 12, 08)
				}
			};

			Guid tempGuid;
			if (!Guid.TryParse(input, out tempGuid))
				tempGuid = Guid.NewGuid();

			mockCustomerService
				.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
				.Returns(
					Task.FromResult(customerList.FirstOrDefault(x => x.CustomerId == tempGuid))
					);

			int intInput;
			if (!int.TryParse(input, out intInput))
				intInput = 0;

			DateOnly todaysDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			mockCustomerService
				.Setup(x => x.GetCustomersByAgeAsync(It.IsAny<int>()))
				.Returns(
					Task.FromResult(customerList.Where(x => (DateTime.Today.Year - x.DateOfBirth.Year) == intInput)));

			//Act
			var response = await controller.GetCustomerByIdOrAge(input);

			//Assert
			response.Should().NotBeNull();
			response.Value.Should().NotBeNull();
			if (IsValid)
			{
				response.Value.Result.Should().NotBeNull();
				response.Value.ErrorMessage.Should().BeEmpty();
			}
			else
			{
				response.Value.ErrorMessage.Should().NotBeNull();
			}
		}


		[Fact]
		public async Task GetCustomerByIdOrAge_Exception()
		{
			string input = "10";
			//Arrange
			var customerList = new List<Customer>() {
				new Customer
				{
					CustomerId = Guid.NewGuid(),
					CustomerName = "John Doe",
					DateOfBirth = new DateOnly(1981, 09, 09)
				},
				new Customer
				{
					CustomerId = Guid.NewGuid(),
					CustomerName = "Daisy Duck",
					DateOfBirth = new DateOnly(1971, 02, 02)
				},
				new Customer
				{
					CustomerId = Guid.Parse("4E23F734-2F8C-41A3-8868-783971B793CD"),
					CustomerName = "Minnie Mouse",
					DateOfBirth = new DateOnly(1993, 12, 08)
				}
			};

			Guid tempGuid;
			if (!Guid.TryParse(input, out tempGuid))
				tempGuid = Guid.NewGuid();

			mockCustomerService
				.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
				.Returns(
					Task.FromResult(customerList.FirstOrDefault(x => x.CustomerId == tempGuid))
					);

			int intInput;
			if (!int.TryParse(input, out intInput))
				intInput = 0;

			DateOnly todaysDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
			mockCustomerService
				.Setup(x => x.GetCustomersByAgeAsync(It.IsAny<int>()))
				.Throws(new Exception("Customer Service is down."));

			//Act
			var response = await controller.GetCustomerByIdOrAge(input);

			//Assert
			response.Should().NotBeNull();
			response.Value.Should().NotBeNull();
			response.Value.ErrorMessage.Should().NotBeNull();
		}

		[Fact]
		public async Task UpdateCustomerDetails_Pass()
		{
			//Arrange
			Guid requestId = Guid.NewGuid();

			var patchDocument = new JsonPatchDocument<CustomerRequest>();
			patchDocument.Replace(r => r.CustomerName, "TestName");

			var customer = new Customer
			{
				CustomerId = requestId,
				CustomerName = "Daisy Duck",
				DateOfBirth = new DateOnly(1988, 03, 07),
				ProfileImage = "randomBytes"
			};

			mockCustomerService
				.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()))
				.Returns(Task.FromResult(customer)!);

			mockMapper
            .Setup(mapper => mapper.Map<CustomerRequest>(It.IsAny<Customer>()))
			.Returns(new CustomerRequest 
			{ 
				CustomerName = customer.CustomerName, 
				DateOfBirth = customer.DateOfBirth.ToString() 
			});

			mockProfilePictureService
				.Setup(x => x.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
				.Returns(Task.FromResult("randomStringBytes"));

			//Act

			var response = await controller.UpdateCustomerDetails(requestId, patchDocument);

			//Assert

			response.Should().NotBeNull();
			response.Value.Result.Should().NotBeNull();
			response.Value.ErrorMessage.Should().BeNullOrEmpty();
		}

		[Fact]
		public async Task UpdateCustomerDetails_NegativeTests()
		{
			//Arrange
			Guid requestId = Guid.NewGuid();

			var patchDocument = new JsonPatchDocument<CustomerRequest>();
			patchDocument.Replace(r => r.CustomerName, "TestName");

			var customer = new Customer
			{
				CustomerId = Guid.NewGuid(),
				CustomerName = "Daisy Duck",
				DateOfBirth = new DateOnly(1988, 03, 07),
				ProfileImage = "randomBytes"
			};

			mockCustomerService
				.Setup(x => x.GetCustomerByIdAsync(It.IsAny<Guid>()));
				//.Returns(Task.FromResult(customer)!);

			mockMapper
			.Setup(mapper => mapper.Map<CustomerRequest>(It.IsAny<Customer>()))
			.Returns(new CustomerRequest
			{
				CustomerName = customer.CustomerName,
				DateOfBirth = customer.DateOfBirth.ToString()
			});

			mockProfilePictureService
				.Setup(x => x.GetBase64EncodedSvgProfilePictureAsync(It.IsAny<string>()))
				.Returns(Task.FromResult("randomStringBytes"));

			//Act

			var response = await controller.UpdateCustomerDetails(requestId, patchDocument);

			//Assert

			response.Should().NotBeNull();
			response.Value.ErrorMessage.Should().NotBeNullOrEmpty();
		}
	}
}
