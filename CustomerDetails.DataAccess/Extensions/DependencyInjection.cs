using CustomerDetails.API.DataAccess.Data;
using CustomerDetails.API.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDetails.DataAccess.Extensions
{
	public static class DependencyInjection
	{
		public static IServiceCollection RegisterDbDependencies(this IServiceCollection services, string connectionString)
		{
			services.AddDbContext<ApplicationDBContext>(
				options =>
				{
					options.UseSqlite(
						connectionString);
				});
			services.AddScoped<ICustomerRepository, CustomerRespository>();
			return services;
		}
	}
}
