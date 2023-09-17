using AutoMapper;
using CustomerDetails.API.DataAccess.DTO;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.API.DataAccess.Repository;
using CustomerDetails.BusinessLogic.Interface;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.RegularExpressions;

namespace CustomerDetails.API.Controllers
{
	[Route("api/customers")]
	[ApiController]
	public class CustomerAPIController : ControllerBase
	{
		private readonly ICustomerService _customerService;
		private readonly IMapper _mapper;
		protected APIResponse _response;
		private readonly IProfilePictureService _pictureService;

		public CustomerAPIController(ICustomerService customerService, IMapper mapper, IProfilePictureService pictureService)
		{
			_customerService = customerService;
			_mapper = mapper;
			_pictureService = pictureService;
			_response = new APIResponse();

		}
		[HttpGet]
		public async Task<ActionResult<APIResponse>> GetCustomersAsync()
		{
			try
			{
				IEnumerable<Customer> getAllCustomers = await _customerService.GetAllAsync();
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

					IEnumerable<Customer> customers = await _customerService.GetCustomersByAgeAsync(age);

					if (customers == null || customers.Count() == 0)
					{
						_response.StatusCode = HttpStatusCode.NotFound;
						_response.ErrorMessage = new List<string>() { "No record found" };
					}

					return Ok(customers);
				}
				
				else if (Guid.TryParse(idOrAge, out Guid customerId))
				{
					var customer = await _customerService.GetCustomerByIdAsync(customerId);

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
		public async Task<ActionResult<APIResponse>> CreateCustomer([FromBody] CreateCustomerRequest createRequest)
		{
			try
			{
				if (createRequest is null)
				{
					_response.ErrorMessage.Add("Request cannot be null.");
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;
				}

				Regex regex = new Regex(@"^[A-Za-z]+( [A-Za-z]+)*$");
				if(!regex.IsMatch(createRequest.CustomerName))
				{
					_response.ErrorMessage.Add("Customer Name cannot have special characters, numbers, leading and trailing spaces and allows one blank space between words.");
				}

				DateOnly dob = new DateOnly();

				if (string.IsNullOrWhiteSpace(createRequest.DateOfBirth))
				{
					_response.ErrorMessage.Add("Customer Date of Birth cannot be null or empty. Please use ISO8601 date format only.");
				}
				else if(!DateOnly.TryParse(createRequest.DateOfBirth, out dob))
				{
					_response.ErrorMessage.Add("Invalid Date Format. Please use ISO8601 date format only.");
				}

				if (_response.ErrorMessage.Any())
				{
					_response.IsSuccess = false;
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;
				}

				Customer customer = new Customer { CustomerName = createRequest.CustomerName, DateOfBirth = dob };

				try
				{
					customer.ProfileImage = await _pictureService.GetBase64EncodedSvgProfilePictureAsync(customer.CustomerName);
				}
				catch (Exception)
				{
					throw;
				}

				await _customerService.AddCustomerAsync(customer);

				_response.Result = _mapper.Map<CustomerDTO>(customer);
				_response.StatusCode = HttpStatusCode.Created;
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				string message = "Exception Occured - " + ex.Message;
				_response.ErrorMessage.Add(message);
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

				var customer = await _customerService.GetCustomerByIdAsync(id);

				if (customer == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					return _response;
				}
				UpdateCustomerDTO updateCustomer = _mapper.Map<UpdateCustomerDTO>(customer);
				updatedCustomerDetails.ApplyTo(updateCustomer);

				Customer customerPatch = _mapper.Map<Customer>(updateCustomer);
				await _customerService.UpdateAsync(customerPatch);


				return NoContent();
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;

		}
		
		[HttpDelete]
		//[ProducesResponseType(StatusCodes.Status204NoContent)] //TODO
		//[ProducesResponseType(StatusCodes.Status400BadRequest)] //TODO
		//[ProducesResponseType(StatusCodes.Status404NotFound)] //TODO
		public async Task<ActionResult<APIResponse>> DeleteCustomer(Guid CustomerId)
		{
			try
			{
				if (CustomerId == Guid.Empty)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					return _response;

				}
				var customer = await _customerService.GetCustomerByIdAsync(CustomerId);

				if (customer == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					return _response;
				}

				await _customerService.RemoveCustomerAsync(customer);

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
