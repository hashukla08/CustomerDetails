using CustomerDetails.API.DataAccess.Entities;

namespace CustomerDetails.BusinessLogic.Interface
{
	public interface ICustomerService
    {
		Task AddCustomerAsync(Customer customer);
		Task<IEnumerable<Customer>> GetAllAsync();
		Task<IEnumerable<Customer>> GetCustomersByAgeAsync(int age);
		Task<Customer?> GetCustomerByIdAsync(Guid id);
		Task<bool> UpdateAsync(Customer customer);
		Task RemoveCustomerAsync(Customer customer);
	}
}
