using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Repository;
using CustomerDetails.BusinessLogic.Services;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerDetails.API.Tests.BusinessLogic.Services
{
	public class CustomerServiceTests
	{
		public CustomerServiceTests()
		{

		}

		[Fact]
		public async Task AddCustomerAsync_Pass()
		{
			//Arrange
			var mockCustomerRepository = new Mock<ICustomerRepository>();	
			
			var customerService = new CustomerService(mockCustomerRepository.Object);
			var customer = new Customer()
			{
				CustomerName = "Harsha Shukla",
				DateOfBirth = new DateOnly(1992, 09, 09)
			};

			mockCustomerRepository.Setup(u => u.CreateAsync(It.IsAny<Customer>()));

			//Act
			await customerService.AddCustomerAsync(customer);

			//Assert
			mockCustomerRepository.Verify(x=>x.CreateAsync(It.IsAny<Customer>()), Times.Once);
		}

		[Fact]
		public async Task AddCustomerAsync_False()
		{
			//Arrange
			var mockCustomerRepository = new Mock<ICustomerRepository>();

			var customerService = new CustomerService(mockCustomerRepository.Object);
			Customer customer = null!;
			mockCustomerRepository.Setup(u => u.CreateAsync(It.IsAny<Customer>()));

			//Act
			await customerService.AddCustomerAsync(customer);

			//Assert
			mockCustomerRepository.Verify(x => x.CreateAsync(It.IsAny<Customer>()), Times.Never);
		}


		[Fact]
		public async Task GetCustomerByAge_Pass()
		{
			//Arrange
			var mockCustomerRepository = new Mock<ICustomerRepository>();

			var customerService = new CustomerService(mockCustomerRepository.Object);
			int age = 42;

			var dbList = new List<Customer>() {
				new Customer{
					CustomerId= new Guid(),
					CustomerName="John Doe",
					DateOfBirth=new DateOnly(1981, 09, 09)}
				};
			

			mockCustomerRepository.Setup(u => u.GetCustomersByAgeAsync(It.IsAny<int>()))
				.Returns(Task.FromResult(dbList.AsEnumerable()));

			//Act
			var response= await customerService.GetCustomersByAgeAsync(age);

			//Assert
			mockCustomerRepository.Verify(x => x.GetCustomersByAgeAsync(It.IsAny<int>()), Times.Once);

			Assert.NotNull(response);
			Assert.Equal(dbList.Where(x=> x.DateOfBirth.AddYears(age).Year == DateTime.Today.Year), response);
		}


		
	}
}