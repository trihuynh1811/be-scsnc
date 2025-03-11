
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;
using Api.Services;

namespace Api.MIddleware
{
    public class RenewTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public RenewTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtTokenService jwtService, IUserService userService)
        {
            if (context?.Request?.Cookies != null && context.Request.Cookies.TryGetValue("X-Access-Token", out var token) && context.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken) && context.Request.Cookies.TryGetValue("X-Email", out var email))
            {
                var user = userService.GetUserByEmail(email);
                if(user == null)
                {
                    await _next(context);
                }

                var jwtTokenHandler = new JwtSecurityTokenHandler();

                if (jwtTokenHandler.CanReadToken(token))
                {
                    var jwtToken = jwtTokenHandler.ReadJwtToken(token);

                    Debug.WriteLine(jwtToken.ValidTo);
                    Debug.WriteLine(DateTime.UtcNow);
                    Debug.WriteLine(jwtToken.ValidTo - DateTime.UtcNow);
                    Debug.WriteLine((jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds);

                    if((jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds > 0)
                    {
                        var expiration = jwtToken.ValidTo;
                        if (expiration < DateTime.UtcNow.AddSeconds(20))
                        {
                            Debug.WriteLine("new token");
                            var newToken = jwtService.GenerateToken(user);

                            context.Response.OnStarting(() =>
                            {
                                context.Response.Cookies.Append("X-Access-Token", newToken, new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None });
                                return Task.CompletedTask;
                            });
                        }

                    }
                }
            }

            await _next(context);
        }
    }
    public static class RenewTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseRenewToken(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RenewTokenMiddleware>();
        }
    }
}
