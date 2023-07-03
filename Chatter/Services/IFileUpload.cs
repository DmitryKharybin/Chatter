namespace AuthenticationApi.Services
{
    //This interface is meant for file upload , First parameter is for entity that will recieve the image
    public interface IFileUpload<T>
    {

        Task UploadFileAsync(T obj, IFormFile selectedFile);

    }
     
    //This delegate will help inject the correct concrete implementation of the service
    public delegate IFileUpload<T> ServiceResolver<T> (string key);
}
