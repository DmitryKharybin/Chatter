namespace AuthenticationApi.Repositories
{
    public interface IUserDataRepository
    {
        Task CreateNewUser(User user);
        Task<User?> GetUserData(string userId);
        Task<bool> ModifyUserData(User user);

    }
}
