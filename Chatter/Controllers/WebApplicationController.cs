using AuthenticationApi.Hubs;
using AuthenticationApi.Models;
using Azure.Core;
using System.Net;

namespace AuthenticationApi.Controllers
{

    //This controller is responsible for retrieving user data



    [Route("api/[controller]"), ApiController]
    public class WebApplicationController : ControllerBase
    {

        private readonly IUserDataRepository userDataRepository;
        private readonly IJwtService jwtService;
        private readonly IHubContext<ChatterHub> chattterHub;
        private readonly IFileHolder fileHolder;
        private readonly IFileUpload<User> userImageUpload;

        public WebApplicationController(IUserDataRepository userDataRepository, IJwtService jwtService, IHubContext<ChatterHub> chattterHub
            , IFileHolder fileHolder, IFileUpload<User> userImageUpload)
        {
            this.userDataRepository = userDataRepository;
            this.jwtService = jwtService;
            this.chattterHub = chattterHub;
            this.fileHolder = fileHolder;
            this.userImageUpload = userImageUpload;
        }


        //Receive token from header , validate it , extract userId , get user from DB , return user
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<User>> GetMyUserData([FromHeader] string authorization)
        {

            string inputToken = authorization.Replace("Bearer", "");
            inputToken = inputToken.Replace(" ", "");


            try
            {

                var user = await GetUserHelper(inputToken);
                if (user == null)
                {
                    return Problem("User not found", "user", 500);
                }

                return user;

            }
            // For now , i want to show exception in console, in future will pass to logging , and custom messages 
            //TODO : add logger

            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 500);
            }

            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                return Problem("Invalid Token", "Token", 500);
            }




        }


        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<User>> GetUserData([FromHeader] string userId)
        {
            var user = await userDataRepository.GetUserAsync(userId);

            if (user == null)
            {
                return Problem("User not found", "User", 404);
            }

            return user;
        }

        //Receive from header the input string ,
        //return the search result for the groups & users containing this string in the name
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public SearchResult GetSearchResults([FromHeader] string str)
        {
            return userDataRepository.GetSearchResult(str);
        }


        //Receive from headre the JWT & id of the user you wish to add to friend list 
        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<SearchResult>> AcceptFriendRequest([FromHeader] string authorization, [FromBody] FriendRequest request)
        {
            if (ModelState.IsValid)
            {

                string inputToken = RemoveBearerHelper(authorization);

                try
                {

                    var user = await GetUserHelper(inputToken);
                    if (user == null)
                    {
                        return Problem("User not found", "user", 500);
                    }

                    var res = await userDataRepository.AcceptFriendRequestAsync(request);

                    if (!res)
                    {
                        return Problem("Somethig went wrong", "Add Friend", 500);
                    }

                    //Update both sender & receiver
                    await chattterHub.Clients.Client(fileHolder.UsersConnections[request.ReceiverId]).SendAsync("RequestUpdate", "FriendRequestAccepted");
                    await chattterHub.Clients.Client(fileHolder.UsersConnections[request.SenderId]).SendAsync("RequestUpdate", "FriendRequestAccepted");
                    return Ok();

                }
                // For now , i want to show exception in console, in future will pass to logging , and custom messages 
                //TODO : add logger
                catch (SecurityTokenExpiredException)
                {
                    return Problem("Token expired", "Token", 500);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                    return Problem("Invalid Token", "Token", 500);
                }
            }

            return Problem("Request is incorrect, check all data is correct, and try again", "Accept friend request", 400);
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<SearchResult>> DeleteFriendRequest([FromBody] FriendRequest request)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var res = await userDataRepository.RemoveFriendRequestAsync(request);
                    Console.WriteLine(res);

                    if (res)
                    {
                        //Update both sender & receiver
                        await chattterHub.Clients.Client(fileHolder.UsersConnections[request.ReceiverId]).SendAsync("RequestUpdate", "FriendRequestDeleted");
                        await chattterHub.Clients.Client(fileHolder.UsersConnections[request.SenderId]).SendAsync("RequestUpdate", "FriendRequestDeleted");
                        return Ok();
                    }

                }

                catch (KeyNotFoundException)
                {
                    Console.WriteLine("User connection id to hub not found in dictionary");
                    return Problem("User connection id to hub not found in dictionary, refresh page", "User not found in hub regisrati", 500);
                }

            }

            return Problem("Request is incorrect, check all data is correct, and try again", "Delete friend request", 400);

        }


        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SendFriendRequest([FromHeader] string authorization, [FromHeader] string targetUserId)
        {

            string inputToken = RemoveBearerHelper(authorization);

            if (ModelState.IsValid)
            {

                try
                {
                    var user = await GetUserHelper(inputToken);

                    if (user == null)
                    {
                        return Problem("User not found", "Friend Request creation", 404);
                    }

                    bool res = await userDataRepository.CreateFriendRequestAsync(user.Id, targetUserId);

                    if (res)
                    {
                        //Update both sender & receiver
                        await chattterHub.Clients.Client(fileHolder.UsersConnections[targetUserId]).SendAsync("RequestUpdate", "NewFriendRequest");
                        return Ok();
                    }

                    return Problem("Faild to create friend request", "Friend Request creation", 500);
                }
                catch (SecurityTokenExpiredException)
                {
                    return Problem("Token expired", "Token", 401);
                }

                catch (KeyNotFoundException)
                {
                    Console.WriteLine("User connection id to hub not found in dictionary");
                    return Problem("User connection id to hub not found in dictionary, refresh page", "User not found in hub regisration", 500);
                }

            }

            return Problem("Data is missing, check all headers and try again", "Headers missing", 400);

        }


        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            try
            {

                if (message != null)
                {
                    if (ModelState.IsValid)
                    {
                        bool res = await userDataRepository.AddMessageAsync(message.Id, message.SenderId, message.ReceiverId, message.Body);


                        if (res)
                        {
                            var sender = await userDataRepository.GetUserAsync(message.SenderId);

                            if (sender != null)
                            {
                                message.Sender = new User { Id = sender.Id, Name = sender.Name, Image = sender.Image };

                                //Update receiving user on message
                                await chattterHub.Clients.Client(fileHolder.UsersConnections[message.ReceiverId]).SendAsync("newMessage", message);
                                return Ok();
                            }

                        }

                    }

                }

            }

            catch (KeyNotFoundException)
            {
                Console.WriteLine("User connection id to hub not found in dictionary");
                return Ok();
            }

            return Problem("Fail to add message", "Message", 500);

        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> MarkMessageAsRead([FromBody] string[] messageIdArr)
        {
            if (messageIdArr != null)
            {
                List<Message> messageList = new List<Message>();
                //var message = await userDataRepository.GetMessageByIdAsync(messageId);

                foreach (string messageId in messageIdArr)
                {
                    messageList.Add(await userDataRepository.GetMessageByIdAsync(messageId));
                }

                if (messageList.Count > 0)
                {
                    foreach (var message in messageList)
                    {
                        await userDataRepository.MarkMessageAsRead(message);
                    }
                    return Ok();
                    //await userDataRepository.MarkMessageAsRead(message);
                }
                else
                {
                    return Problem("Message was not found", "Retrieve message", 404);
                }
            }

            return Problem("Incorrect or missing header", "Check header", 400);

        }


        //Get chat with specific user
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<Message>>> GetUserChat([FromHeader] string authorization, [FromHeader] string secondParticipantId)
        {
            string inputToken = RemoveBearerHelper(authorization);

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await GetUserHelper(inputToken);

                    if (user == null)
                    {
                        return Problem("User not found", "Friend Request creation", 404);
                    }

                    var chatMessages = await userDataRepository.GetChatByUserIdAsync(user.Id, secondParticipantId);

                    if (chatMessages == null)
                    {
                        return Problem("Chat not found", "Chat lookup", 404);
                    }

                    return chatMessages.ToList();

                }
                catch (SecurityTokenExpiredException)
                {
                    return Problem("Token expired", "Token", 401);
                }

            }

            return Problem("Second participant data missing, check all data is valid , and try again", "Get chat", 400);

        }


        //Get All chats 
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<Message>>> GetAllUserChats([FromHeader] string authorization)
        {
            string inputToken = RemoveBearerHelper(authorization);

            try
            {
                var user = await GetUserHelper(inputToken);

                if (user == null)
                {
                    return Problem("User not found", "Friend Request creation", 404);
                }

                var chatMessages = await userDataRepository.GetAllUserChatsAsync(user.Id);

                if (chatMessages == null)
                {
                    return Problem("No chats found", "Chat lookup", 404);
                }

                return chatMessages.ToList();

            }
            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 401);
            }

        }


        [HttpGet]
        [Authorize]
        [Route("[action]")]
        //Get Friends
        public async Task<ActionResult<IEnumerable<User>>> GetFriends([FromHeader] string authorization)
        {
            string inputToken = RemoveBearerHelper(authorization);

            try
            {
                var user = await GetUserHelper(inputToken);

                if (user == null)
                {
                    return Problem("User not found", "Friend Request creation", 404);
                }
                //TODO: filter special chars(just in case)
                var requests = await userDataRepository.GetFriends(user.Id);

                if (requests == null)
                {
                    return Problem("Friend request not found", "Friend reqeust", 404);
                }

                return requests.ToList();
            }
            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 401);
            }
        }

        //Get Friend reqeust, if not found return status 404 & error message
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<FriendRequest>>> GetFriendRequests([FromHeader] string authorization)
        {
            string inputToken = RemoveBearerHelper(authorization);

            try
            {
                var user = await GetUserHelper(inputToken);

                if (user == null)
                {
                    return Problem("User not found", "Friend Request creation", 404);
                }
                //TODO: filter special chars(just in case)
                var requests = await userDataRepository.GetFriendRequestByUserId(user.Id);

                if (requests == null)
                {
                    return Problem("Friend request not found", "Friend reqeust", 404);
                }

                return requests.ToList();
            }
            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 401);
            }

        }

        //Get Friend reqeust, if not found return status 404 & error message
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<IEnumerable<User>>> GetChatParticipants([FromHeader] string authorization)
        {
            string inputToken = RemoveBearerHelper(authorization);

            try
            {
                var user = await GetUserHelper(inputToken);

                if (user == null)
                {
                    return Problem("User not found", "Friend Request creation", 404);
                }
                //TODO: filter special chars(just in case)
                var chatParticipantsId = await userDataRepository.GetAllChatParticipants(user.Id);

                if (chatParticipantsId == null)
                {
                    return Problem("Friend request not found", "Friend reqeust", 404);
                }

                return chatParticipantsId.ToList();
            }
            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 401);
            }

        }

        //Return userData , using the token, if token expired , expired exception will be thrown
        private async Task<User?> GetUserHelper(string token)
        {

            var jwtToken = jwtService.ValidateToken(token);


            var userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId")?.Value;

            if (userId == null)
            {
                return null;
            }

            var user = await userDataRepository.GetUserAsync(userId);
            return user!;


        }

        // Helper method for removing 'Bearer' from Authorization header
        private string RemoveBearerHelper(string authHeader)
        {
            string inputToken = authHeader.Replace("Bearer", "");
            inputToken = inputToken.Replace(" ", "");
            return inputToken;
        }


        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult> UpdateUserData([FromHeader] string authorization, [FromBody] User updatedUser)
        {
            string inputToken = RemoveBearerHelper(authorization);

            if (ModelState.IsValid)
            {
                if (updatedUser.Name != string.Empty)
                {
                    try
                    {
                        var user = await GetUserHelper(inputToken);

                        if (user == null)
                        {
                            return Problem("User not found", "Friend Request creation", 404);
                        }

                        //Updated user should have matching id to token to prevent update to wrong user
                        if (user.Id != updatedUser.Id)
                        {
                            return Problem("User Id does not match token claim", "Token", 400);
                        }

                        user.Name = updatedUser.Name;
                        user.Email = updatedUser.Email;

                        var updateRes = await userDataRepository.UpdateUserAsync(user);

                        if (!updateRes)
                        {
                            return Problem("faild to update user data", "update user data", 500);
                        }

                        return Ok();

                    }
                    catch (SecurityTokenExpiredException)
                    {
                        return Problem("Token expired", "Token", 401);
                    }

                }

            }

            return Problem("Second participant data missing, check all data is valid , and try again", "Get chat", 400);

        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult> UpdateUserImage([FromHeader] string authorization, [FromForm] IFormFile newImage)
        {
            string inputToken = RemoveBearerHelper(authorization);

            try
            {
                var user = await GetUserHelper(inputToken);

                if (user == null)
                {
                    return Problem("User not found", "Friend Request creation", 404);
                }

                var updateRes = await userImageUpload.UploadFileAsync(user, newImage);

                if (updateRes == null)
                {
                    return Problem("faild to update user image", "update user image", 500);
                }

                return Ok(updateRes);

            }
            catch (SecurityTokenExpiredException)
            {
                return Problem("Token expired", "Token", 401);
            }

        }


    }
}
