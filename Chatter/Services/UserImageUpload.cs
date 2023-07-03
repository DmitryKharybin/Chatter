namespace AuthenticationApi.Services
{
    public class UserImageUpload : IFileUpload<User>
    {

        private readonly IUserDataRepository dataRepository;



        public UserImageUpload(IUserDataRepository dataRepository)
        {

            this.dataRepository = dataRepository;

        }

        public async Task UploadFileAsync(User user, IFormFile selectedFile)
        {

            if (user != null && selectedFile != null && selectedFile.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await selectedFile.CopyToAsync(stream);
                    user.Image = stream.ToArray();
                    await dataRepository.UpdateUserAsync(user);

                }
            }


        }

    }
}
