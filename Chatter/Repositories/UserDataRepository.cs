using AuthenticationApi.Models;
using System.Runtime.InteropServices;

namespace AuthenticationApi.Repositories
{
    public class UserDataRepository : IUserDataRepository
    {
        UserDataContext userDataContext;


        public UserDataRepository(UserDataContext userDataContext)
        {

            this.userDataContext = userDataContext;

        }

        //When a user add a friend , both users get added to each other lists 
        public async Task<bool> AddFriendAsync(User user, string friendId)
        {
            if(user == null || friendId == string.Empty)
            {
                return false;
            }

            //Try and find the friend, in not found then perform no action & notify that user not found
            var friend = userDataContext.Users.Find(friendId);

            if(friend == null)
            {
                return false;
            }

            if(user.Friends == null)
            {
                user.Friends = new List<User>();
            }

            if (friend.Friends == null)
            {
                friend.Friends = new List<User>();
            }


            user.Friends.Add(friend);
            friend.Friends.Add(user);

            
            return await userDataContext.SaveChangesAsync() > 0;

          

        }

        public async Task AddGroupAsync(Group group)
        {
            if(group != null)
            {
                userDataContext.Groups.Add(group);
                await userDataContext.SaveChangesAsync();
            }
           
        }

        public async Task AddPostAsync(Post post)
        {
            if(post != null)
            {
                userDataContext.Posts.Add(post);
                await userDataContext.SaveChangesAsync();
            }
          
        }


        public async Task AddUserAsync(User user)
        {
            if(user != null)
            {
                await userDataContext.AddAsync(user);

                await userDataContext.SaveChangesAsync();
            }
           
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await userDataContext.Users!.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            userDataContext.Users.Remove(user);
            return await userDataContext.SaveChangesAsync() > 0;

        }

        public async Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName)
        {
            var groups = await userDataContext.Groups.Where(g => g.Name == groupName).ToListAsync();
            return groups;
        }


        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
        {
            var user = await userDataContext.Users!.FindAsync(userId);

            if(user != null)
            {
                if(user.Posts != null)
                {
                    return user.Posts;
                }
            }

            return null;
        }

        public async Task<User?> GetUserAsync(string userId)
        {

            return await userDataContext.Users.SingleOrDefaultAsync(user => user.Id == userId);


        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            if(group != null)
            {
                userDataContext.Groups.Add(group);
                return await userDataContext.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (await userDataContext.Users.AsNoTracking().AnyAsync(u => u.Id == user.Id))
            {
                return false;
            }

            userDataContext.Users.Update(user);
            return await userDataContext.SaveChangesAsync() > 0;
        }
    }
}
