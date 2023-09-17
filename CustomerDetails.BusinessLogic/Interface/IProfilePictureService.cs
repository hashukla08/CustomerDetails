namespace CustomerDetails.BusinessLogic.Interface
{
    public interface IProfilePictureService
    {
        Task<string> GetBase64EncodedSvgProfilePictureAsync(string customerName);
    }
}
