namespace AuthenticationApi.Repositories
{
    public class UserDataRepository : IUserDataRepository
    {
        UserDataContext userDataContext;


        public UserDataRepository(UserDataContext userDataContext)
        {

            this.userDataContext = userDataContext;

        }


        public async Task CreateNewUser(User user)
        {
           await userDataContext.AddAsync(user);

           await userDataContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserData(string userId)
        {

            return await userDataContext.Users.SingleOrDefaultAsync(user => user.Id == userId);

            
        }

        public Task<bool> ModifyUserData(User user)
        {
            throw new NotImplementedException();
        }
    }
}
