using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;

namespace SporttiporssiAPI.Helpers
{
    public class JwtHelper
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireDays;

        public JwtHelper(string key, string issuer, string audience, int expireDays)
        {
            _key = key;
            _issuer = issuer;
            _audience = audience;
            _expireDays = expireDays;
        }

        public string GenerateToken(string email)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(_expireDays),
                Issuer = _issuer,
                IssuedAt = DateTime.UtcNow,
                Audience = _audience,               
                SigningCredentials = creds,
            };
           
            var tokenHandler = new JsonWebTokenHandler();
            var jwtToken = tokenHandler.CreateToken(tokenDescriptor);
            return jwtToken;
        }

        public Task<TokenValidationResult> ValidateToken(string token)
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
                var principal = tokenHandler.ValidateTokenAsync(token, validationParameters);
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Validation failed");
                return null;
            }          
        }
    }
}
