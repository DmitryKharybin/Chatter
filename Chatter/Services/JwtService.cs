

namespace AuthenticationApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration configuration;

        IUserDataRepository userDataRepository;


        public JwtService(IConfiguration configuration, IUserDataRepository userDataRepository)
        {
            this.configuration = configuration;
            this.userDataRepository = userDataRepository;

        }

        public JwtSecurityToken GenerateToken(List<Claim> authClaims)
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
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
           

                var jwtToken = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidateAudience"],
                    ClockSkew = TimeSpan.Zero

                }, out SecurityToken validatedToken);


            return jwtToken;
        }
         
     
    }
}
