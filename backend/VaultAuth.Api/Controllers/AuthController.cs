using Microsoft.AspNetCore.Authorization;
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
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, IPasswordService passwordService, IConfiguration configuration, TokenService tokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request, IFormFile? profileImage)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest(new { message = "The Email address already exists, please try again." });

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                
            };
            user.HashedPassword = _passwordService.HashPassword(user, request.Password);

            // Profile image upload to local file system
            if (profileImage != null && profileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Invalid file type. Only .jpg and .png are allowed.");

                const long maxFileSize = 2 * 1024 * 1024; 
                if (profileImage.Length > maxFileSize)
                    return BadRequest("File too large. Maximum allowed size is 2 MB.");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ImageURL = $"/images/{fileName}";
            }
            else
            {
                user.ImageURL = "/images/default.png";
            }


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new RegisterResponse { Message = "User registered successfully." });
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

                return Unauthorized(new
                {
                    message = "Account is temporarily locked. Please try again later.",
                    lockoutEnd = user.LockoutEnd 
                });
            }

            var isValid = _passwordService.VerifyPassword(user, request.Password);
            if (!isValid)
            {
                user.FailedLognAtp++;

                if (user.FailedLognAtp >= 3)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLognAtp = 0;
                    await _context.SaveChangesAsync();

                    return Unauthorized(new { message = "Account is temporarily locked.", lockoutEnd = user.LockoutEnd });
                }

                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Invalid email or password." }); 
            }

            // Reset failed attempts on success
            user.FailedLognAtp = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]));
            await _context.SaveChangesAsync();

            return Ok(new { accessToken, refreshToken });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]));
            await _context.SaveChangesAsync();

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token." });

            var userId = int.Parse(userIdClaim); 

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePassword dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return BadRequest("User not found");

            // Hash the new password
            user.HashedPassword = _passwordService.HashPassword(user, dto.NewPassword);

            await _context.SaveChangesAsync();
            return Ok("Password updated successfully");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token." });

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.ID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.ImageURL,
                user.RefreshTokenExpiry
            });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUser dto, IFormFile? profileImage)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token." });

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Check that Email doesnt already exist.
            if (!string.IsNullOrEmpty(dto.Email))
            {
                bool emailExists = _context.Users.Any(u => u.Email == dto.Email && u.ID != user.ID);
                if (emailExists)
                    return BadRequest(new { message = "This email address is already in use. Please try another one." });

                user.Email = dto.Email;
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;

            // Profile image upload to local file system
            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{profileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Save URL to DB
                user.ImageURL = $"/images/{fileName}";
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated successfully." });
            }
            catch (DbUpdateException ex)
            {
                // Check if it's a unique constraint violation
                if (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
                {
                    return BadRequest(new { message = "This email address is already in use. Please try another one." });
                }

                // Fallback for other DB errors
                return StatusCode(500, new { message = "An unexpected error occurred while updating the user." });
            }

        }





    }
}
