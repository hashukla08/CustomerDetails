using AutoMapper;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.DataAccess.Models;
using System.Runtime;

namespace CustomerDetails.API.DataAccess.Models
{
    public class MappingConfig:Profile
	{
        public MappingConfig()
        {
			CreateMap<Customer, CustomerRequest>().ReverseMap();

		}
	}
}
