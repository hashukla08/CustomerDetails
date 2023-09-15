using CustomerDetails.API.Services;
using CustomerDetails.BusinessLogic.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerDetails.BusinessLogic.Extensions
{
	public static class DependencyInjection
	{
		public static IServiceCollection RegisterServices(this IServiceCollection services)
		{
			services.AddHttpClient<IProfilePictureService, ProfilePictureService>();
			services.AddScoped<IProfilePictureService, ProfilePictureService>();

			return services;
		}
	}
}
