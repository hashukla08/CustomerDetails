using AutoMapper;
using CustomerDetails.API.DataAccess.Entities;
using CustomerDetails.API.DataAccess.Models;
using CustomerDetails.BusinessLogic.Interface;
using CustomerDetails.DataAccess.Models;
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

		private static readonly Regex _nameRegex = new Regex(@"^[A-Za-z]+( [A-Za-z]+)*$");

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
				if (getAllCustomers != null && getAllCustomers.Any())
				{
					_response.StatusCode = HttpStatusCode.OK;
					_response.Result = getAllCustomers;
				}
				else
				{
					_response.StatusCode = HttpStatusCode.NoContent;
					
				}
				
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
						_response.ErrorMessage.Add("Customers with given age not found");
						return _response;
					}
					_response.Result = customers;
					return _response;
				}

				else if (Guid.TryParse(idOrAge, out Guid customerId))
				{
					var customer = await _customerService.GetCustomerByIdAsync(customerId);

					if (customer == null)
					{
						_response.StatusCode = HttpStatusCode.NotFound;
						_response.ErrorMessage.Add("Customer with given customer id not found");
						return _response;
					}

					_response.Result = customer;
					return _response;
				}
				else
				{
					_response.StatusCode=HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessage.Add("Invalid input");
					return _response;
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.ErrorMessage = new List<string> { ex.ToString() };
				return _response;
			}
			
		}


		[HttpPost]
		public async Task<ActionResult<APIResponse>> CreateCustomer([FromBody] CustomerRequest createRequest)
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

				if (!_nameRegex.IsMatch(createRequest.CustomerName))
				{
					_response.ErrorMessage.Add("Customer Name cannot have special characters, numbers, leading and trailing spaces and allows one blank space between words.");
				}

				DateOnly dob = new DateOnly();

				if (string.IsNullOrWhiteSpace(createRequest.DateOfBirth))
				{
					_response.ErrorMessage.Add("Customer Date of Birth cannot be null or empty. Please use ISO8601 date format only.");
				}
				else if (!DateOnly.TryParse(createRequest.DateOfBirth, out dob))
				{
					_response.ErrorMessage.Add("Invalid Date Format. Please use ISO8601 date format only.");
				}

				if(dob.ToDateTime(new TimeOnly()) >= DateTime.Today)
				{
					_response.ErrorMessage.Add("Date of Birth cannot be a future date.");
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

				_response.Result = customer;
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
		public async Task<ActionResult<APIResponse>> UpdateCustomerDetails(Guid id, JsonPatchDocument<CustomerRequest> updatedCustomerDetails)
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
					_response.IsSuccess = false;
					_response.ErrorMessage.Add("Data not found.");
					return _response;
				}

				CustomerRequest customerToUpdate = _mapper.Map<CustomerRequest>(customer);
				updatedCustomerDetails.ApplyTo(customerToUpdate);
				bool reloadProfilePicture = false;
				var dateOfBirth = DateOnly.Parse(customerToUpdate.DateOfBirth);

				if (!_nameRegex.IsMatch(customerToUpdate.CustomerName))
				{
					_response.ErrorMessage.Add("Customer Name cannot have special characters, numbers, leading and trailing spaces and allows one blank space between words.");
				}

				if (dateOfBirth.ToDateTime(new TimeOnly()) >= DateTime.Today)
				{
					_response.ErrorMessage.Add("Date of Birth cannot be a future date.");
				}

				if (_response.ErrorMessage is not null && _response.ErrorMessage.Any())
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					return _response;
				}

				if (!customer.CustomerName.Equals(customerToUpdate.CustomerName, StringComparison.OrdinalIgnoreCase))
					reloadProfilePicture = true;

				customer.CustomerName = customerToUpdate.CustomerName;
				customer.DateOfBirth = dateOfBirth;


				if(reloadProfilePicture)
				{
					customer.ProfileImage= await _pictureService.GetBase64EncodedSvgProfilePictureAsync(customer.CustomerName);
				}
				await _customerService.UpdateAsync(customer);

				_response.Result = customer;
				_response.StatusCode = HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				_response.StatusCode=HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessage = new List<string> { ex.ToString() };
			}
			return _response;

		}

		
	}
}
