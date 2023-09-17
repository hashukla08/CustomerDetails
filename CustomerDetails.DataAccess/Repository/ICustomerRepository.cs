using CustomerDetails.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CustomerDetails.API.DataAccess.Repository
{
	public interface ICustomerRepository:IRepository<Customer>
	{
		Task<bool> UpdateAsync(Customer customer);
		Task<IEnumerable<Customer>> GetCustomersByAgeAsync(int age);
		Task<Customer?> GetCustomerByIdAsync(Guid id);
	}

}