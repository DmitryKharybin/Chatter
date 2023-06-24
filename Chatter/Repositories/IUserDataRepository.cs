namespace AuthenticationApi.Repositories
{
    public interface IUserDataRepository
    {
        Task CreateNewUserAsync(User user);

        Task<User?> GetUserDataAsync(string userId);

        Task<bool> ModifyUserDataAsync(User user);

        //There could be multiple groups with same name
        Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName);

        //User id is guid , therefore string
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId);

        Task<IEnumerable<Post>> GetPostsByGroupIdAsync(int groupId);
    }
}
