using AutoMapper;
using CustomerDetails.API.DataAccess.DTO;
using CustomerDetails.API.DataAccess.Entities;
using System.Runtime;

namespace CustomerDetails.API.DataAccess.Models
{
	public class MappingConfig:Profile
	{
        public MappingConfig()
        {
			CreateMap<Customer, CustomerDTO>().ReverseMap();
			CreateMap<Customer, CreateCustomerDTO>().ReverseMap();
			CreateMap<CustomerDTO, CreateCustomerDTO>().ReverseMap();

			
			CreateMap<Customer, UpdateCustomerDTO>().ReverseMap();
			CreateMap<CustomerDTO, UpdateCustomerDTO>().ReverseMap();

		}
	}
}
