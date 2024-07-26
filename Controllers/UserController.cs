using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using SporttiporssiAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using SporttiporssiAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SporttiporssiAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly JwtHelper _jwtHelper;

        public UserController(ApplicationDbContext context, HttpClient httpClient, JwtHelper jwtHelper)
        {
            _context = context;
            _httpClient = httpClient;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
                if(user == null)
                {
                    return Unauthorized("Invalid email or password");
                }
                var hashedPassword = HashPassword(request.Password, user.Salt);
                if(hashedPassword != user.PasswordHash)
                {
                    return Unauthorized("Invalid email or password");
                }
                var token = _jwtHelper.GenerateToken(user.Email);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error..");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RegisterNewUser([FromBody] RegisterRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if(user != null)
            {
                return StatusCode(205, "This email is already registered.");
            }
            try
            {
                string salt = GenerateSalt();
                string passwordHash = HashPassword(request.Password, salt);

                var newUser = new User
                {
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    Role = "user"
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return Ok("New user registered");
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Internal server error..");
            }
        }
        [Authorize]
        [HttpGet("validate-token")]
        public async Task<ActionResult> ValidateToken()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            if(authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var principal = _jwtHelper.ValidateToken(token);
                    if(principal != null)
                    {
                        return Ok();
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                catch
                {
                    return Unauthorized();
                }
            }
            return Unauthorized();
        }
      
        private string GenerateSalt(int size = 16)
        {           
            var buffer = new byte[size];
            RandomNumberGenerator.Create().GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        private string HashPassword(string password, string salt)
        {
            var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            byte[] saltedPasswordBytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
            byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hashBytes);
        }


    }
}
