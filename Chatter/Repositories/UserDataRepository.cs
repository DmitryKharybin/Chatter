using AuthenticationApi.Models;
using Azure.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
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

        //Add Friend & delete the request from DB

        public async Task<bool> AcceptFriendRequestAsync(FriendRequest request)
        {

            var req = userDataContext.FriendRequests.Where(r => r.Id == request.Id).FirstOrDefault();

            if (req == null)
            {
                return false;
            }

            var res = await AddFriendAsync(request.ReceiverId, request.SenderId);

            if (!res)
            {
                return false;
            }

            userDataContext.FriendRequests.Remove(req);

            return await userDataContext.SaveChangesAsync() > 0;
        }

        //Remove the request from DB 
        public async Task<bool> RemoveFriendRequestAsync(FriendRequest request)
        {
            if (request != null)
            {
                var foundRequest = userDataContext.FriendRequests.Where(r => r.Id == request.Id).FirstOrDefault();

                if (foundRequest != null)
                {
                    userDataContext.FriendRequests.Remove(foundRequest);
                    return await userDataContext.SaveChangesAsync() > 0;
                }

            }

            return false;
        }

        //When a user add a friend , both users get added to each other lists 
        public async Task<bool> AddFriendAsync(string userId, string friendId)
        {
            if (userId == string.Empty || friendId == string.Empty)
            {
                return false;
            }



            //Lazy loading does not support no tracking queries, eager loading was used instead

            var friend = userDataContext.Users
               .Where(f => f.Id == friendId)
               .Include(f => f.Friends)
               .AsNoTracking()
               .FirstOrDefault();



            var user = userDataContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            //var friend = userDataContext.Users.Where(f => f.Id == friendId).FirstOrDefault();


            //If not found , return false;
            if (friend == null || user == null)
            {
                return false;
            }

            if (user.Friends == null)
            {
                user.Friends = new List<User>();
            }

            if (friend.Friends == null)
            {
                friend.Friends = new List<User>();
            }


            user.Friends.Add(friend);
            friend.Friends.Add(user);



            userDataContext.Entry(friend).State = EntityState.Modified;
            //userDataContext.Entry(user).State = EntityState.Modified;

            bool hasChanged = userDataContext.ChangeTracker.HasChanges();

            return await userDataContext.SaveChangesAsync() > 0;



        }

        public async Task AddGroupAsync(Group group)
        {
            if (group != null)
            {
                userDataContext.Groups.Add(group);
                await userDataContext.SaveChangesAsync();
            }

        }

        public async Task AddPostAsync(Post post)
        {
            if (post != null)
            {
                userDataContext.Posts.Add(post);
                await userDataContext.SaveChangesAsync();
            }

        }


        public async Task<bool> AddUserAsync(User user)
        {

            if (user != null)
            {
                var duplicateUser = userDataContext.Users.AsNoTracking().Where(u => u.Id == user.Id).FirstOrDefault();

                if (duplicateUser == null)
                {
                    await userDataContext.AddAsync(user);
                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }

            return false;

        }

        public async Task<bool> CreateFriendRequestAsync(string senderId, string targetUserId)
        {
            if (senderId != null || targetUserId != null)
            {

                var duplicateRequest = userDataContext.FriendRequests.AsNoTracking().Where(r => r.SenderId == senderId && r.ReceiverId == targetUserId).FirstOrDefault();

                if (duplicateRequest == null)
                {
                    userDataContext.FriendRequests.Add(new FriendRequest { SenderId = senderId, ReceiverId = targetUserId, Date = DateTime.Now });

                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }

            return false;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (userId != null)
            {
                var user = await userDataContext.Users!.FindAsync(userId);

                if (user != null)
                {
                    userDataContext.Users.Remove(user);
                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }
            return false;

        }

        public async Task<IEnumerable<Group>> GetGroupsByNameAsync(string groupName)
        {
            var groups = await userDataContext.Groups.Where(g => g.Name == groupName).ToListAsync();
            return groups;
        }

        public IEnumerable<Group> GetGroupsContainingString(string str)
        {
            return userDataContext.Groups.Where(group => group.Name.Contains(str) == true).ToList();
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
        {
            var user = await userDataContext.Users!.FindAsync(userId);

            if (user != null)
            {
                if (user.Posts != null)
                {
                    return user.Posts;
                }
            }

            return null;
        }


        //Return search result, including users and groups
        public SearchResult GetSearchResult(string str)
        {

            IEnumerable<User> users = GetUsersContainingString(str);
            IEnumerable<Group> groups = GetGroupsContainingString(str);

            SearchResult searchResult = new SearchResult() { Users = users, Groups = groups };

            return searchResult;
        }

        public async Task<User?> GetUserAsync(string userId)
        {
            return await userDataContext.Users.SingleOrDefaultAsync(user => user.Id == userId);
        }

        //Meant to get all users who's name contain certain string
        public IEnumerable<User> GetUsersContainingString(string str)
        {
            return userDataContext.Users.Where(user => user.Name.Contains(str) == true).ToList();
        }

        public Task<bool> RemoveFriendRequestAsync(User user, string requestingUser)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            if (group != null)
            {
                bool groupFound = await userDataContext.Groups.AsNoTracking().AnyAsync(g => g.Id == group.Id);

                if (groupFound)
                {
                    userDataContext.Groups.Add(group);
                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }

            return false;
        }


        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user != null)
            {
                bool userFound = await userDataContext.Users.AsNoTracking().AnyAsync(u => u.Id == user.Id);
                if (userFound)
                {
                    userDataContext.Users.Update(user);
                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }

            return false;
        }

        //Return list of friend requests for user
        public async Task<IEnumerable<FriendRequest>> GetFriendRequestByUserId(string userId)
        {
            return await userDataContext.FriendRequests.Where(r => r.ReceiverId == userId).Select(r => new FriendRequest
            {
                Id = r.Id,
                Date = r.Date,
                ReceiverId = r.ReceiverId,
                SenderId = r.SenderId,
                Sender = new User()
                {
                    Id = userId,
                    Name = r.Sender.Name,
                    Image = r.Sender.Image
                }
            }).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetFriends(string userId)
        {
            if (userId != null)
            {
                var user = await userDataContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

                if (user.Friends != null)
                {
                    return user.Friends;
                }
            }

            return null;
        }

        public async Task<bool> AddMessageAsync(string id, string senderId, string receiverId, string messageBody)
        {
            if ((senderId != null || receiverId != null) && messageBody != string.Empty)
            {
                var duplicateMessage = userDataContext.Messages.AsNoTracking().Where(r => r.Id == id).FirstOrDefault();

                if (duplicateMessage == null)
                {
                    //userDataContext.Messages.Add(new Message { Id = id, SenderId = senderId, ReceiverId = receiverId, Date = DateTime.Now, Body = messageBody});

                    var receiver = userDataContext.Users.FirstOrDefault(u => u.Id == receiverId);
                    var sender = userDataContext.Users.FirstOrDefault(u => u.Id == senderId);

                    if (receiver != null && sender != null)
                    {
                        Message message = new Message { Id = id, SenderId = senderId, ReceiverId = receiverId, Date = DateTime.Now, Body = messageBody, IsRead = false };
                        receiver.ReceivedMessages.Add(message);
                        sender.SentMessages.Add(message);
                    }

                    return await userDataContext.SaveChangesAsync() > 0;
                }
            }

            return false;
        }

        //Get all chats (sent & received messages)
        public async Task<IEnumerable<Message>> GetAllUserChatsAsync(string userId)
        {
            if (userId != null)
            {
                return await userDataContext.Messages.Where(r => r.ReceiverId == userId || r.SenderId == userId).ToListAsync();
            }

            return null;
        }

        //Get All participants of existing chats with user
        public async Task<IEnumerable<User>> GetAllChatParticipants(string userId)
        {
            List<string> userIdCollection = new List<string>();
            

            var user = await userDataContext.Users.Where(u => u.Id == userId).SingleOrDefaultAsync();
            if (user != null)
            {
                var receiverIdCollection = user.SentMessages?.Select(m => m.ReceiverId).ToList();
                var senderIdCollection = user.ReceivedMessages?.Select(m => m.SenderId).ToList();

                userIdCollection.AddRange(receiverIdCollection);
                userIdCollection.AddRange(senderIdCollection);

                userIdCollection = userIdCollection.Distinct().ToList();

                return userDataContext.Users.Where(u => userIdCollection.Contains(u.Id))
                    .Select(u => new User
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Image = u.Image,
                        Email = u.Email,
                    }).ToList();
            }

            return null;
        }

        //Get all messages in specific chat
        public async Task<IEnumerable<Message>> GetChatByUserIdAsync(string userId, string secondUserId)
        {
            if (userId != null && secondUserId != null)
            {
                //return await userDataContext.Messages.Where(r => (r.ReceiverId == userId && r.SenderId == secondUserId) ||
                //(r.ReceiverId == secondUserId && r.SenderId == userId)).ToListAsync();

                return await userDataContext.Messages.Where(r => (r.ReceiverId == userId && r.SenderId == secondUserId) ||
                (r.ReceiverId == secondUserId && r.SenderId == userId)).Select(m => new Message
                {
                    Id = m.Id,
                    ReceiverId = m.ReceiverId,
                    SenderId = m.SenderId,
                    Body = m.Body,
                    IsRead = m.IsRead,
                    Date = m.Date,
                }).OrderBy(m => m.Date)
                .ToListAsync();
            }

            return null;
        }


        //Mark the message as read
        public async Task MarkMessageAsRead(Message message)
        {
            message.IsRead = true;

            await userDataContext.SaveChangesAsync();
        }

        public async Task<Message> GetMessageByIdAsync(string messageId)
        {
            return await userDataContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
        }
    }
}
