using Microsoft.IdentityModel.Tokens;

namespace SporttiporssiAPI.Interfaces
{
    public interface ITokenValidator
    {
        Task<TokenValidationResult> ValidateToken(string token);
    }
}
