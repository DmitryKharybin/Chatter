namespace AuthenticationApi.Repositories
{
    public class UserDataRepository : IUserDataRepository
    {
        UserDataContext userDataContext;


        public UserDataRepository(UserDataContext userDataContext)
        {

            this.userDataContext = userDataContext;

        }


        public async Task CreateNewUserAsync(User user)
        {
        
           await userDataContext.AddAsync(user);

           await userDataContext.SaveChangesAsync();
        }

        public Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByGroupIdAsync(int groupId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserDataAsync(string userId)
        {

            return await userDataContext.Users.SingleOrDefaultAsync(user => user.Id == userId);

            
        }

        public Task<bool> ModifyUserDataAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
