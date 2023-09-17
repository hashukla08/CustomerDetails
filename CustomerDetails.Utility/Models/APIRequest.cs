using static Constants;

namespace CustomerDetails.API.DataAccess.Models
{
	public class APIRequest
	{
			public APIType requestType { get; set; } = APIType.GET;
			public string requestURL { get; set; } = string.Empty;
			public object requestData { get; set; }
		
	}
}
