namespace AuthenticationApi.Services
{
     public interface IJwtService
    {
        JwtSecurityToken GenerateToken(List<Claim> authClaims);

        ClaimsPrincipal ValidateToken(string token);
    }
}
