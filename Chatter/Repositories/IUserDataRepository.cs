namespace AuthenticationApi.Repositories
{
    public interface IUserDataRepository
    {
        Task AddUserAsync(User user);

        Task<User?> GetUserAsync(string userId);

        Task<bool> DeleteUserAsync(string userId);

        Task AddPostAsync(Post post);

        Task<bool> AddFriendAsync(User user, string friendId);

        Task<bool> UpdateUserAsync(User user);

        Task AddGroupAsync(Group group);

        Task<bool> UpdateGroupAsync(Group group);

        //There could be multiple groups with same name
        Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName);

        //User id is guid , therefore string
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId);

    }
}
