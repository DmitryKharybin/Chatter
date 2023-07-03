namespace AuthenticationApi.Controllers
{

    //This controller is responsible for retrieving user data
    
    
    [Route("api/[controller]")]
    [ApiController]
    public class WebApplicationController : ControllerBase
    {

        private readonly IUserDataRepository userDataRepository;
        private readonly IJwtService jwtService;

        public WebApplicationController(IUserDataRepository userDataRepository, IJwtService jwtService)
        {
            this.userDataRepository = userDataRepository;
            this.jwtService = jwtService;
        }


        //Receive token from header , validate it , extract userId , get user from DB , return user
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<User>> GetUserData([FromHeader] string authorization)
        {

            string inputToken = authorization.Replace("Bearer", "");
            inputToken = inputToken.Replace(" ", "");
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {

                var jwtToken = jwtService.ValidateToken(inputToken);

                var userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId")?.Value;

                if (userId == null)
                {
                    return Problem("Missing user identification","UerId",500);
                }

                var user = await userDataRepository.GetUserAsync(userId);
                return user!;

            }
            // For now , i want to show exception in console, in future will pass to logging , and custom messages 
            //TODO : add logger
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                return Problem("Invalid Token", "Token", 500);
            }




        }
    }
}
