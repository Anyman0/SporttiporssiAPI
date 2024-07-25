using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SporttiporssiAPI.Interfaces;
using System.Security.Claims;
using System.Text;

namespace SporttiporssiAPI.Services
{
    public class TokenValidator : ITokenValidator
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenValidator(string key, string issuer, string audience)
        {
            _key = key;
            _issuer = issuer;
            _audience = audience;
        }

        public async Task<TokenValidationResult> ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var tokenHandler = new JsonWebTokenHandler();
            var jwtToken = tokenHandler.ReadJsonWebToken(token);
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = key
                };
                var principal = await tokenHandler.ValidateTokenAsync(token, validationParameters);
                return principal;
            }
            catch (Exception)
            {
                return new TokenValidationResult { IsValid = false };
            }
        }
    }
}
