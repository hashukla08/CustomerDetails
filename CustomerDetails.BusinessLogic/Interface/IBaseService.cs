using CustomerDetails.API.DataAccess.Models;

namespace CustomerDetails.BusinessLogic.Interface
{
    public interface IBaseService
    {
        APIResponse response { get; set; }

        Task<string> SendAsync(APIRequest requests);
    }
}
