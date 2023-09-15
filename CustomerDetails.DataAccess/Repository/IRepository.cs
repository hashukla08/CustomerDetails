using System.Collections;
using System.Linq.Expressions;

namespace CustomerDetails.API.DataAccess.Repository
{
	public interface IRepository<T> where T: class
	{
		Task CreateAsync(T entity);
		Task<IEnumerable<T>> GetAllAsync();
		
		Task RemoveAsync(T entity);
	    Task SaveAsync();
		
	}
}

