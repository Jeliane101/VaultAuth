using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VaultAuth.Api.Data;
using VaultAuth.Api.DTOs;
using VaultAuth.Api.Models;
using VaultAuth.Api.Services;



namespace VaultAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IPasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "The Email address already exists, please try again." });

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            user.HashedPassword = _passwordService.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new RegisterResponse());
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Account is temporarily locked. Please try again later." });
            }

            var isValid = _passwordService.VerifyPassword(user, request.Password);
            if (!isValid)
            {
                user.FailedLognAtp++;

                if (user.FailedLognAtp >= 3)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // lock for 15 minutes
                    user.FailedLognAtp = 0; // reset counter after lock
                }

                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Reset failed attempts on success
            user.FailedLognAtp = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            // JWT generation logic here...
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim("userId", user.ID.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(10); // valid for 10 minutes

            await _context.SaveChangesAsync();

            // In production: send via email
            return Ok(new { token = user.ResetPasswordToken });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired token." });

            user.HashedPassword = _passwordService.HashPassword(user, newPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "The password reset successfully." });
        }




    }
}
