using System.Security.Claims;

namespace ACLAuthorization.Services
{
    public interface IJWTTokenServices
    {
        public enum JWTTokenType
        {
            Access, Refresh
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims);
        public string GenerateRefreshToken(IEnumerable<Claim> claims);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, JWTTokenType type);
    }
}
