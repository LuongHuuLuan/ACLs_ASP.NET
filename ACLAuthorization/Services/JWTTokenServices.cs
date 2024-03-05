using Microsoft.IdentityModel.Tokens;
using static ACLAuthorization.Services.IJWTTokenServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ACLAuthorization.Services
{
    public class JWTTokenServices: IJWTTokenServices
    {
        private readonly IConfiguration _config;

        public JWTTokenServices(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = _config["Jwt:AccessKey"];
            // mã hóa key
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            // Ký vào signingKey
            var signingCredential = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var tokenOptions = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: signingCredential,
                    claims: claims
                );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return tokenString;
        }

        public string GenerateRefreshToken(IEnumerable<Claim> claims)
        {
            var key = _config["Jwt:RefreshKey"];
            // mã hóa key
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            // Ký vào signingKey
            var signinCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var tokenOptions = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: signinCredentials,
                    claims: claims
                );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return tokenString;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, JWTTokenType type)
        {
            // lấy key từ app settings
            var key = _config["Jwt:AccessKey"];
            if (type == JWTTokenType.Refresh)
            {
                key = _config["Jwt:RefreshKey"];
            }
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var tokenValidationParameters = new TokenValidationParameters
            {
                // yêu cầu có kiểm tra issue default = true
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                // Yêu cầu kiểm tra về audience default = true
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                // chỉ ra token phải cấu hình expire
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Chỉ ra key mà token sẽ dùng sau này
                IssuerSigningKey = signingKey,
                RequireSignedTokens = true
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
    }
}
