using Infrastructure.Data;
using Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Refreshtoken> refreshtokensSet;


        public JwtTokenService(IUserService userService, IConfiguration configuration, ApplicationDbContext context)
        {
            _userService = userService;
            _configuration = configuration; 
            _context = context;
            refreshtokensSet = _context.Set<Refreshtoken>();
        }

        public void AddToken(string token, User user)
        {
            try
            {
                if (_userService.GetAllUsers().Where(x => x.UserId == user.UserId) != null)
                {
                    var t = new Refreshtoken
                    {
                        RefreshToken = token,
                        UserId = user.UserId,
                        Expiration = DateTime.UtcNow.AddDays(7)
                    };

                    if (_context.Refreshtokens.Where(x => x.UserId == user.UserId).Count() > 0)
                    {
                        t = _context.Refreshtokens.FirstOrDefault(x => x.UserId == user.UserId);
                        if(t.Expiration < DateTime.UtcNow)
                        {
                            t.Expiration = DateTime.UtcNow.AddDays(7);
                            t.RefreshToken = token;
                            refreshtokensSet.Update(t);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        refreshtokensSet.Add(t);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message);
            }

        }

        public string GenerateToken(User? user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:secret"]);
            var securityKey = new SymmetricSecurityKey(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:accessTokenExpiration"])),
                Issuer = _configuration["Jwt:issuer"],
                Audience = _configuration["Jwt:audience"],
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }

    }
}
