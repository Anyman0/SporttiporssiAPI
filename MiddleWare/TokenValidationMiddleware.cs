using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SporttiporssiAPI.Interfaces;
using System.Security.Claims;

namespace SporttiporssiAPI.MiddleWare
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenValidator _tokenValidator;

        public TokenValidationMiddleware(RequestDelegate next, ITokenValidator tokenValidator)
        {
            _next = next;
            _tokenValidator = tokenValidator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                var validationResult = await _tokenValidator.ValidateToken(token);
                if(validationResult.IsValid)
                {
                    var tokenHandler = new JsonWebTokenHandler();
                    var jwtToken = tokenHandler.ReadJsonWebToken(token);
                    var claims = jwtToken.Claims;
                    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
                    context.User = claimsPrincipal;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid token");
                    return;
                }
            }
            await _next(context);
        }
    }
}
