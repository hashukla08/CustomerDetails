using CustomerDetails.API.DataAccess.Data;
using CustomerDetails.API.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CustomerDetails.API.DataAccess.Repository
{
	public class CustomerRespository : Repository<Customer>, ICustomerRepository
	{
		private readonly ApplicationDBContext _db;
		public CustomerRespository(ApplicationDBContext Db) : base(Db)
		{
			_db=Db;
		}
		

		public async Task<Customer> UpdateAsync(Customer customer)
		{
		    _db.Customers.Update(customer);
			await _db.SaveChangesAsync();
			return customer;
			
		}

		public async Task<IEnumerable<Customer>> GetAsync(int age)
		{
			DateOnly CustomerBirthYear = DateOnly.FromDateTime(DateTime.Today.AddYears(-age)); ;
			return await _db.Customers.Where(customer => customer.DateOfBirth == CustomerBirthYear).ToListAsync();

		}
		public async Task<Customer?> GetAsync(Guid id)
		{
			return await _db.Customers.Where(customer => customer.CustomerId == id).AsNoTracking().FirstOrDefaultAsync();
		}
	}
}
