namespace AuthenticationApi.Services
{
    public class GroupImageUpload : IFileUpload<Group>
    {
        private readonly IUserDataRepository dataRepository;



        public GroupImageUpload(IUserDataRepository dataRepository)
        {

            this.dataRepository = dataRepository;

        }

        public async Task UploadFileAsync(Group group, IFormFile selectedFile)
        {

            if (group != null && selectedFile != null && selectedFile.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await selectedFile.CopyToAsync(stream);
                    group.Image = stream.ToArray();
                    await dataRepository.UpdateGroupAsync(group);

                }
            }


        }
    }
}
