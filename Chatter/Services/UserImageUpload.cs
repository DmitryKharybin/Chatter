namespace AuthenticationApi.Services
{
    public class UserImageUpload : IFileUpload<User>
    {

        private readonly IUserDataRepository dataRepository;



        public UserImageUpload(IUserDataRepository dataRepository)
        {

            this.dataRepository = dataRepository;

        }

        public async Task<byte[]> UploadFileAsync(User user, IFormFile selectedFile)
        {

            if (user != null && selectedFile != null && selectedFile.Length > 0)
            {
                if (TestCorrectFileExtension(selectedFile))
                {
                    using (var stream = new MemoryStream())
                    {
                        await selectedFile.CopyToAsync(stream);
                        user.Image = stream.ToArray();
                        if(await dataRepository.UpdateUserAsync(user))
                        {
                            return stream.ToArray();
                        }
                    }
                }

                return null;
                
            }

            return null;
        }

        private bool TestCorrectFileExtension(IFormFile formFile)
        {
            string[] extensions = new string[] { "jpg", "png", "jpeg" };
            var fileExtension = formFile.FileName.Split('.')[1];
            if (extensions.Contains(fileExtension))
            {
               return true;
            }

            return false;
           
        }

    }
}
