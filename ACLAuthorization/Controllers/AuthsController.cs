using ACLAuthorization.Resources;
using ACLAuthorization.Helper;
using ACLAuthorization.Services;
using ACLAuthorization.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using ACLAuthorization.Models;
using System.Linq;

namespace ACLAuthorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthsController : ControllerBase
    {

        private readonly ILogger<AuthsController> _logger;
        private readonly Context _context;
        private readonly IConfiguration _config;
        private readonly IJWTTokenServices _token;

        public AuthsController(ILogger<AuthsController> logger, Context context, IConfiguration config, IJWTTokenServices token)
        {
            _logger = logger;
            _context = context;
            _config = config;
            _token = token;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthenticatedResponse>> Login([FromBody] LoginRequest loginModel)
        {
            // hash md5
            var encodeMD5Password = Md5.GennerateMD5(loginModel.Password);
            var user = _context.users.Include(user => user.roles).SingleOrDefault(user => user.Username == loginModel.Username && user.password == Md5.GennerateMD5(loginModel.Password));
            if (user != null)
            {
                // create claims for token
                var jti = Guid.NewGuid().ToString();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, jti)
                };
                foreach (var role in user.roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }

                var accessToken = _token.GenerateAccessToken(claims);
                var refreshToken = _token.GenerateRefreshToken(claims);
                // get expire time of token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(refreshToken);
                var expires = jwtToken.ValidTo;
                //// save token to db
                //var outStandingToken = new OutstandingToken
                //{
                //    Jti = jti,
                //    Token = refreshToken,
                //    ExpiresAt = expires,
                //    CreatedAt = DateTime.Now,
                //    Account = user.Account
                //};
                //_context.OutstandingTokens.Add(outStandingToken);
                //await _context.SaveChangesAsync();

                return new AuthenticatedResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            else
            {
                return new JsonResult(new { message = "Invalid username or password" });
            }
        }

        [HttpGet("AllUsers")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUser()
        {
            var users = await _context.users.Include(user => user.roles)
                                            .ThenInclude(role => role.Permissions).ToListAsync();
            var response = users.Select(user => new UserResponse(user)).ToList();
            return response;
        }

        [HttpGet("UserInfo/{id}")]
        public async Task<ActionResult<UserResponse>> GetUserInfo(int id)
        {
            var account = await _context.users.Include(user => user.roles)
                                                .ThenInclude(role => role.Permissions)
                                                .Where(user => user.Id == id).FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return new UserResponse(account);
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserResponse>> PostStatus(RegisterRequest registerDTO)
        {
            try
            {
                if (_context.users.Any(account => account.Username.Equals(registerDTO.Username)))
                {
                    return BadRequest("Username is exist!");
                }
                else
                {

                    var user = new User
                    {
                        Username = registerDTO.Username,
                        password = Md5.GennerateMD5(registerDTO.Password)
                    };

                    var userRole = await _context.roles.FirstOrDefaultAsync(role => role.Name == "user");
                    user.roles.Add(userRole);

                    _context.users.Add(user);


                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetUserInfo", new { id = user.Id }, new UserResponse(user));
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<AuthenticatedResponse>> Refresh([FromBody] RefreshTokenRequest refreshTokenModel)
        {
            if (refreshTokenModel is null)
                return BadRequest();

            var principal = _token.GetPrincipalFromExpiredToken(refreshTokenModel.RefreshToken, IJWTTokenServices.JWTTokenType.Refresh);

            //// Check token in blacklist
            //var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            //var blacklistedToken = await _context.BlacklistedTokens
            //                        .Include(bt => bt.Token)
            //                        .SingleOrDefaultAsync(bt => bt.Token.Jti == jti.Value);
            //if (blacklistedToken != null)
            //    return BadRequest("Token in blacklist");

            //var tokenHandler = new JwtSecurityTokenHandler();
            //var jwtToken = tokenHandler.ReadJwtToken(refreshTokenModel.RefreshToken);
            //var expires = jwtToken.ValidTo;
            //var outstandingToken = await _context.OutstandingTokens.SingleOrDefaultAsync(e => e.Jti == jti.Value);

            //if (outstandingToken.ExpiresAt <= DateTime.Now)
            //    return BadRequest("Invalid client request");

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(refreshTokenModel.RefreshToken);
            var expires = jwtToken.ValidTo;
            if (expires <= DateTime.Now)
                return BadRequest("Invalid client request");

            var newAccessToken = _token.GenerateAccessToken(principal.Claims);
            var newRefreshToken = refreshTokenModel.RefreshToken;

            return new AuthenticatedResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest refreshTokenModel)
        {
            //if (refreshTokenModel is null)
            //    return BadRequest();

            //var principal = _token.GetPrincipalFromExpiredToken(refreshTokenModel.RefreshToken, IJWTTokenServices.JWTTokenType.Refresh);
            ////var username = principal.Identity.Name;
            //// Check token in blacklist
            //var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            //var blacklistedToken = await _context.BlacklistedTokens
            //                        .Include(bt => bt.Token)
            //                        .SingleOrDefaultAsync(bt => bt.Token.Jti == jti.Value);
            //if (blacklistedToken != null)
            //    return BadRequest("Token already in blacklist");

            //var outstandingToken = await _context.OutstandingTokens.SingleOrDefaultAsync(e => e.Jti == jti.Value);

            //_context.BlacklistedTokens.Add(new BlacklistedToken
            //{
            //    BlacklistedAt = DateTime.Now,
            //    Token = outstandingToken
            //}); ;

            //_context.SaveChanges();

            return NoContent();
        }
    }
}