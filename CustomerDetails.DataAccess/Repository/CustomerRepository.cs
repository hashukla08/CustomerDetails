using CustomerDetails.API.DataAccess.Data;
using CustomerDetails.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CustomerDetails.API.DataAccess.Repository
{
	public class CustomerRepository : Repository<Customer>, ICustomerRepository
	{
		private readonly ApplicationDBContext _db;
		public CustomerRepository(ApplicationDBContext Db) : base(Db)
		{
			_db=Db;
		}
		

		public async Task<bool> UpdateAsync(Customer customer)
		{
			try
			{
				_db.Customers.Update(customer);
				await _db.SaveChangesAsync();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			
			
		}

		public async Task<IEnumerable<Customer>> GetCustomersByAgeAsync(int age)
		{
			DateOnly CustomerBirthYear = DateOnly.FromDateTime(DateTime.Today.AddYears(-age)); ;
			return await _db.Customers.Where(customer => customer.DateOfBirth == CustomerBirthYear).ToListAsync();

		}
		public async Task<Customer?> GetCustomerByIdAsync(Guid id)
		{
			return await _db.Customers.Where(customer => customer.CustomerId == id).AsNoTracking().FirstOrDefaultAsync();
		}
	}
}
