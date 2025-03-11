using Api.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Auth0.AspNetCore.Authentication;
using System.Diagnostics;


namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserList([FromServices] IUserService userService)
        {
            return Ok(new { user_list = userService.GetAllUsers() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginCookie([FromBody] AuthDTO user,
[FromServices] IUserService userService, [FromServices] IJwtTokenService tokenService, [FromServices] IConfiguration configuration)
        {
            var storedUser = userService.GetUserByEmail(user?.Email);
            if (storedUser == null)
            {
                return BadRequest();
            }
            if (!userService.IsAuthenticated(user?.Password, storedUser?.Password))
            {
                return Unauthorized();
            }

            var accessToken = tokenService.GenerateToken(storedUser);
            var refreshToken = tokenService.GenerateRefreshToken();
            tokenService.AddToken(refreshToken, storedUser);

            Response.Cookies.Append("X-Access-Token", accessToken, new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
            Response.Cookies.Append("X-Email", user.Email, new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
            Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });

            return Ok(new { access_token = accessToken, refresh_token = refreshToken });

            return Ok();

        }

        [HttpGet("dummy")]
        //[Authorize(Roles = "Admin")]
        [Authorize ("read-data")]
        public IActionResult Dummy()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var token = "";
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            return Ok(new { auth0_acess_token = token });
        }
    }

    public record AuthDTO(string? Email, string? Password);
}
