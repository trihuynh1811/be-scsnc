using Infrastructure.Model;
using System.Security.Claims;

namespace Api.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User? user);
        string GenerateRefreshToken();
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
        void AddToken(string token, User user);

    }
}
