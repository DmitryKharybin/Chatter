using Azure.Core;

namespace AuthenticationApi.Repositories
{
    public interface IUserDataRepository
    {
        Task<bool> AddUserAsync(User user);

        Task<User?> GetUserAsync(string userId);

        Task<bool> DeleteUserAsync(string userId);

        Task AddPostAsync(Post post);

        Task<bool> AddFriendAsync(string userId, string friendId);

        Task<bool> AcceptFriendRequestAsync(FriendRequest request);

        Task<bool> RemoveFriendRequestAsync(FriendRequest request);

        Task<bool> CreateFriendRequestAsync(string senderId, string targetUserId);

        Task<bool> RemoveFriendRequestAsync(User user, string targetUserId);

        Task<bool> UpdateUserAsync(User user);


        Task<bool> AddMessageAsync(string senderId, string receiverId, string messageBody);

        //Get all chats the user have
        Task<IEnumerable<Message>> GetAllUserChatsAsync(string userId);

        //Get chat with specific user
        Task<IEnumerable<Message>> GetChatByUserIdAsync(string userId ,string receiverId);


        Task<IEnumerable<User>> GetFriends(string userId);

        Task<IEnumerable<FriendRequest>> GetFriendRequestByUserId(string userId);

        //Meant to get all users who's name contain certain string
        IEnumerable<User> GetUsersContainingString(string str);

        //Meant to get all Groups who's name contain certain string
        IEnumerable<Group> GetGroupsContainingString(string str);

        Task AddGroupAsync(Group group);

        Task<bool> UpdateGroupAsync(Group group);

        //There could be multiple groups with same name
        Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName);

        //User id is guid , therefore string
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId);

        SearchResult GetSearchResult(string str);

    }
}
