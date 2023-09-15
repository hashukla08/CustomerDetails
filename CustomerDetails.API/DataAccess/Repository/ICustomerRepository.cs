using CustomerDetails.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CustomerDetails.API.DataAccess.Repository
{
	public interface ICustomerRepository:IRepository<Customer>
	{
		Task<Customer> UpdateAsync(Customer customer);
		Task<IEnumerable<Customer>> GetAsync(int age);
		Task<Customer?> GetAsync(Guid id);
	}

}