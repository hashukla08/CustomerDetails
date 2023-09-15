using System.Net;

namespace CustomerDetails.API.DataAccess.Models
{
	public class APIResponse
	{
		public HttpStatusCode StatusCode { get; set; }
			public bool IsSuccess { get; set; } = true;
			public List<string> ErrorMessage { get; set; }= new List<string>();
			public object Result { get; set; } = new object();

		
	}
}
