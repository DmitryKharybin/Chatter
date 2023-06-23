
namespace AuthenticationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<IdentityUser> signInManager;

        private readonly IConfiguration configuration;
        IUserDataRepository userDataRepository;


        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, IUserDataRepository userDataRepository)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.userDataRepository = userDataRepository;

        }
        

        /// <summary>
        /// Fetch The UserName & Password In case it exists, Send Them to Creation of JWT  
        /// </summary>
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
                //Try to sign in
                var loginRes = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (loginRes.Succeeded)
                {

                    var user = await userManager.FindByNameAsync(model.UserName);


                    if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                    //var res = await loginManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                    //if (user != null && res.Succeeded)
                    {
                        var userRoles = await userManager.GetRolesAsync(user);

                        var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                   new Claim("userId", user.Id)
                };

                        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                        //Get instance of JWT 
                        var token = GetToken(authClaims);


                        return Ok(new
                        {
                            //Write The JWT
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                        });


                    }

                }

            }

            return Unauthorized();

        }


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
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = createUserRes.Errors.ToString() });
                //return Problem("Email already in use", "Registration", 500);

            }


            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "User" });
            }

            var addToRoleRes = await userManager.AddToRoleAsync(newUser, "User");

            if (!addToRoleRes.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = addToRoleRes.Errors.ToString() });
                //return Problem("Email already in use", "Registration", 500);

            }


            await userDataRepository.CreateNewUser(new User { Id = model.Id.ToString()!, Email = model.Email, Name = model.Name });

            return StatusCode(StatusCodes.Status201Created);

        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {


            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User Registration faild, Check your input and try again" });
            }

            //Look for user matching name
            var user = await userManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User already exist" });
            }

            var email = await userManager.FindByEmailAsync(model.Email);

            if (email != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "Email already in use" });
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
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "Failed to Create User" });
            }


            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            }

            var addToRoleRes = await userManager.AddToRoleAsync(newUser, "Admin");

            if (!addToRoleRes.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = $"Faild to add user to role of Admin" });
            }

            await userDataRepository.CreateNewUser(new User { Id = model.Id.ToString()!, Email = model.Email, Name = model.Name });


            return StatusCode(StatusCodes.Status201Created);


        }


        //Find user By Id 
        [HttpDelete]
        [Route("[action]/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {

            var user = await userManager.FindByIdAsync(id);

            if (user != null)
            {
                var res = await userManager.DeleteAsync(user);

                if (res.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, new ResponseModel { Status = "Success", Message = "User Deleted Successfully" });
                }

                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "Faild to Delete user" });
                }
            }


            return StatusCode(StatusCodes.Status404NotFound, new ResponseModel { Status = "Error", Message = "User not found" });
        }





        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidateAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }




    }
}
