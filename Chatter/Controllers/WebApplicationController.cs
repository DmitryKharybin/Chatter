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

        public WebApplicationController(IUserDataRepository userDataRepository, IJwtService jwtService, IHubContext<ChatterHub> chattterHub
            , IFileHolder fileHolder)
        {
            this.userDataRepository = userDataRepository;
            this.jwtService = jwtService;
            this.chattterHub = chattterHub;
            this.fileHolder = fileHolder;
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

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<ActionResult<SearchResult>> DeleteFriendRequest([FromBody] FriendRequest request)
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

            return Problem("Something went wrong", "Delete friend request", 500);

        }


        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SendFriendRequest([FromHeader] string authorization, [FromHeader] string targetUserId)
        {

            string inputToken = RemoveBearerHelper(authorization);

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

        }


        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SendMessage([FromHeader] Message message)
        {

            if (message != null)
            {

                bool res = await userDataRepository.AddMessageAsync(message.SenderId, message.ReceiverId, message.Body);

                if (res)
                {
                    //Update receiving user on message
                    await chattterHub.Clients.Client(fileHolder.UsersConnections[message.ReceiverId]).SendAsync("newMessage", message.SenderId);
                    return Ok();
                }

            }

            return Problem("Fail to add message", "Message", 500);

        }


        //TODO : Finish user chats actions
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        //Get Friends
        public async Task<ActionResult<IEnumerable<Message>>> GetUserChat([FromHeader] string authorization)
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


    }
}
