namespace CustomerDetails.BusinessLogic.Interface
{
    public interface IProfilePictureService
    {
        Task<string> GetProfilePictureAsync(string CustomerName);
    }
}
