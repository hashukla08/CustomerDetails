using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Repository;
using CustomerDetails.BusinessLogic.Interface;

namespace CustomerDetails.BusinessLogic.Services
{
	public class CustomerService : ICustomerService
	{
		private readonly ICustomerRepository _customerRepository;
		public CustomerService(ICustomerRepository customerRepository)
		{
			_customerRepository = customerRepository;
		}

		public async Task AddCustomerAsync(Customer customer)
		{
			if(customer == null)
			{
				return;
			}
			await _customerRepository.CreateAsync(customer);
		}

		public async Task<IEnumerable<Customer>> GetAllAsync()
		{
			return await _customerRepository.GetAllAsync();
		}

		public async Task<IEnumerable<Customer>> GetCustomersByAgeAsync(int age)
		{
			var customers = await _customerRepository.GetCustomersByAgeAsync(age);
			return customers;
		}

		public async Task<Customer?> GetCustomerByIdAsync(Guid id)
		{
			var customer = await _customerRepository.GetCustomerByIdAsync(id);
			return customer;
		}

		public async Task<bool> UpdateAsync(Customer customer)
		{
			return await _customerRepository.UpdateAsync(customer);
		}

		public async Task RemoveCustomerAsync(Customer customer)
		{
			await _customerRepository.RemoveAsync(customer);
		}
	}
}
