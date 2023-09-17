using CustomerDetails.API.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CustomerDetails.API.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDBContext _db;
		internal DbSet<T> DbSet;
		public Repository(ApplicationDBContext Db)
		{
			_db = Db;
			this.DbSet = _db.Set<T>();
		}

		public async Task CreateAsync(T entity)
		{
			await DbSet.AddAsync(entity);
			await SaveAsync();
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			IQueryable<T> query = DbSet;
			return await query.ToListAsync();
		}


		public async Task RemoveAsync(T entity)
		{
		    _db.Remove(entity);
			await SaveAsync();
		}

		public async Task SaveAsync()
		{
			await _db.SaveChangesAsync();
		}


	}
}
