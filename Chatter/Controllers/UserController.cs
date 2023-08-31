
namespace AuthenticationApi.Controllers
{
    //This controller is responsible for user : Registration, Login, Deletion .
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;


        private readonly IUserDataRepository userDataRepository;
        private readonly IJwtService jwtService;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment env;



        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager
            , IUserDataRepository userDataRepository, IJwtService jwtService, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)

        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.userDataRepository = userDataRepository;
            this.jwtService = jwtService;
            this.env = env;

        }


        /// <summary>
        /// Fetch The UserName & Password In case it exists, Send Them to Creation of JWT  
        ///// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 



        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            //Validation of the login model
            if (ModelState.IsValid)
            {
                //First check if account name exist , then if it does , check if password match.
                var user = await userManager.FindByNameAsync(model.UserName);


                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))

                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                         {
                            new Claim(ClaimTypes.Name, user.UserName!),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim("userId", user.Id)
                         };

                    authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                    //Generate a Json Web Token
                    var token = jwtService.GenerateToken(authClaims);


                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));

                }
            }

            return Unauthorized();

        }



        //Register a user with provided input information 

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {

            if (!ModelState.IsValid)
            {
                return Problem("User Registration faild, Check your input and try again", "Registration", 500);
            }


            //Look for user matching name
            var user = await userManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                return Problem("User already exist", "Registration", 500);
            }

            var email = await userManager.FindByEmailAsync(model.Email);

            if (email != null)
            {
                return Problem("Email already in use", "Registration", 500);
            }


            //Generate user a new Guid id
            model.Id = Guid.NewGuid().ToString();


            IdentityUser newUser = new IdentityUser
            {
                Id = model.Id.ToString()!,
                UserName = model.UserName,
                //Make sure that no other userName with same name can be created (Difference in upper/lower case)
                NormalizedUserName = model.UserName.ToLower(),
                Email = model.Email
            };

            var createUserRes = await userManager.CreateAsync(newUser, model.Password);

            if (!createUserRes.Succeeded)
            {
                return Problem("Faild to Create User", "User Creation", 500);
            }


            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "User" });
            }

            var addToRoleRes = await userManager.AddToRoleAsync(newUser, "User");

            if (!addToRoleRes.Succeeded)
            {
                return Problem("Failed to add user to role : User", "Role asigning", 500);

            }
            //Get default image as bit array
            var image = DefaultImageToByeArr();
            await userDataRepository.AddUserAsync(new User { Id = model.Id.ToString()!, Email = model.Email, Name = model.Name, Gender = model.Gender, Image = image });

            return StatusCode(StatusCodes.Status201Created);

        }

        //Turn image into a bit array 
        private byte[] DefaultImageToByeArr()
        {
            string defaultImagePath = Path.Combine(env.WebRootPath,"Pictures", "DefaultAvatar.png");
            byte[] imageBytes;

            using (FileStream fileStream = new FileStream(defaultImagePath, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);

                    imageBytes = memoryStream.ToArray();
                }
            }

            //System.IO.File.ReadAllBytes(defaultImagePath);

            return imageBytes;
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {


            if (!ModelState.IsValid)
            {
                return Problem("Input Faild validation", "Input invalid", 500);
            }

            //Look for user matching name
            var user = await userManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                return Problem("User already in use", "User name duplication", 500);
            }

            var email = await userManager.FindByEmailAsync(model.Email);

            if (email != null)
            {
                return Problem("Mail already in use", "Mail duplication", 500);
            }

            //Generate user a new Guid id
            model.Id = Guid.NewGuid().ToString();


            IdentityUser newUser = new IdentityUser
            {
                Id = model.Id.ToString()!,
                UserName = model.UserName,
                //Make sure that no other userName with same name can be created (Difference in upper/lower case)
                NormalizedUserName = model.UserName.ToLower(),
                Email = model.Email
            };

            var createUserRes = await userManager.CreateAsync(newUser, model.Password);

            if (!createUserRes.Succeeded)
            {
                return Problem("Faild to Create User", "User Creation", 500);
            }


            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            }

            var addToRoleRes = await userManager.AddToRoleAsync(newUser, "Admin");

            if (!addToRoleRes.Succeeded)
            {
                return Problem("Failed to add user to role : Admin", "Role asigning", 500);
            }

            await userDataRepository.AddUserAsync(new User { Id = model.Id.ToString()!, Email = model.Email, Name = model.Name, Gender = model.Gender });


            return StatusCode(StatusCodes.Status201Created);


        }




        //Find user By Id (Get id from token) , Delete user & user data
        //If user not found , return internal error code 500 
        //IF claim not found , return internal error code 500
        //If user found , delete user (userManager) & delete user data (userDataRepository)
        //If delete fail , return error 500 + message
        [HttpDelete]
        [Route("[action]/{id}")]
        public async Task<IActionResult> DeleteUser([FromHeader] string authorization)
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
                    return Problem("Missing user identification", "UerId", 500);
                }

                var user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var userDelRes = await userManager.DeleteAsync(user);

                    if (userDelRes.Succeeded)
                    {
                        var dataDelRes = await userDataRepository.DeleteUserAsync(userId);

                        if (!dataDelRes)
                        {
                            return Problem("Faild to Delete user data", " User data Deletion", 500);
                        }

                        return StatusCode(StatusCodes.Status200OK, new ResponseModel { Status = "Success", Message = "User Deleted Successfully" });
                    }

                    else
                    {
                        return Problem("Faild to Delete user", "User Deletion", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Problem("User not found", "User Deletion", 404);

            }

            return Problem("User not found", "User Deletion", 404);
        }



    }


}

