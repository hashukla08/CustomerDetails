using AutoMapper;
using CustomerDetails.API.DataAccess.DTO;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.API.DataAccess.Repository;
using CustomerDetails.BusinessLogic.Interface;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CustomerDetails.API.Controllers
{
	[Route("api/customers")]
	[ApiController]
	public class CustomerAPIController : ControllerBase
	{
		private readonly ICustomerRepository _repository;
		private readonly IMapper _mapper;
		protected APIResponse _response;
		private readonly IProfilePictureService _pictureService;

		public CustomerAPIController(ICustomerRepository repository, IMapper mapper, IProfilePictureService pictureService)
		{
			_repository = repository;
			_mapper = mapper;
			_pictureService = pictureService;
			this._response = new APIResponse();

		}
		[HttpGet]
		public async Task<ActionResult<APIResponse>> GetCustomersAsync()
		{
			try
			{
				IEnumerable<Customer> getAllCustomers = await _repository.GetAllAsync();
				_response.StatusCode = HttpStatusCode.OK;
				_response.Result = getAllCustomers;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;
		}

		[HttpGet("{idOrAge}")]
		public async Task<ActionResult<APIResponse>> GetCustomerByIdOrAge(string idOrAge)
		{
			try
			{
				if (int.TryParse(idOrAge, out int age))
				{

					IEnumerable<Customer> customers = await _repository.GetAsync(age);

					if (customers == null || customers.Count() == 0)
					{
						_response.StatusCode = HttpStatusCode.NotFound;
						_response.ErrorMessage = new List<string>() { "No record found" };
					}

					return Ok(customers);
				}
				
				else if (Guid.TryParse(idOrAge, out Guid customerId))
				{
					Customer customer = await _repository.GetAsync(customerId);

					if (customer == null)
					{
						_response.StatusCode = HttpStatusCode.NotFound;
					}

					return Ok(customer);
				}
				else
				{
					//TODO: Invalid input
					return BadRequest("Invalid ID or age.");
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;
		}


		[HttpPost]
		public async Task<ActionResult<APIResponse>> CreateCustomer([FromBody] CreateCustomerDTO createDTO)
		{
			try
			{
				if (createDTO == null)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;
				}


				Customer customer = _mapper.Map<Customer>(createDTO);

				await _repository.CreateAsync(customer);				
				await UpdateCustomerProfilePictureAsync(customer);
				_response.Result = _mapper.Map<CustomerDTO>(customer);
				_response.StatusCode = HttpStatusCode.Created;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;
		}

		[HttpPatch]
		public async Task<ActionResult<APIResponse>> UpdateCustomerDetails(Guid id, JsonPatchDocument<UpdateCustomerDTO> updatedCustomerDetails)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;
				}

				var customer = await _repository.GetAsync(id);

				if (customer == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					return _response;
				}
				UpdateCustomerDTO updateCustomer = _mapper.Map<UpdateCustomerDTO>(customer);
				updatedCustomerDetails.ApplyTo(updateCustomer);

				Customer customerPatch = _mapper.Map<Customer>(updateCustomer);
				await _repository.UpdateAsync(customerPatch);


				return NoContent();
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;

		}
		private async Task UpdateCustomerProfilePictureAsync(Customer customer)
		{

			var svgData=await _pictureService.GetProfilePictureAsync(customer.CustomerName);

			byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(svgData);
			string profilePicture = Convert.ToBase64String(dataBytes);

			customer.ProfileImage = profilePicture;
			await _repository.UpdateAsync(customer);
			
		}

		[HttpDelete]
		//[ProducesResponseType(StatusCodes.Status204NoContent)]
		//[ProducesResponseType(StatusCodes.Status400BadRequest)]
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<APIResponse>> DeleteCustomer(Guid CustomerId)
		{
			try
			{
				if (CustomerId == Guid.Empty)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;

				}
				var customer = await _repository.GetAsync(CustomerId);

				if (customer == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					return _response;
				}

				await _repository.RemoveAsync(customer);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;
		}

	}
}
