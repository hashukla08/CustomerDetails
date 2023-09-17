using CustomerDetails.BusinessLogic.Interface;
using CustomerDetails.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerDetails.BusinessLogic.Extensions
{
	public static class DependencyInjection
	{
		public static IServiceCollection RegisterServices(this IServiceCollection services)
		{
			services.AddHttpClient<IProfilePictureService, ProfilePictureService>();
			services.AddScoped<IProfilePictureService, ProfilePictureService>();

			services.AddScoped<ICustomerService, CustomerService>();

			return services;
		}
	}
}
