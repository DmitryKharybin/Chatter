namespace AuthenticationApi.Controllers
{
    
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

                return await userDataRepository.GetUserData(userId);

            }
            // For now , i want to show exception in console, in future will pass to logging , and custom messages 
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                return Problem("Invalid Token", "Token", 500);
            }




        }
    }
}
