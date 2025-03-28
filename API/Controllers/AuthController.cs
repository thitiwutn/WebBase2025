using API.Database.Tables;
using API.Database;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized();

            var jwt = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            AddRefreshToken(user.Id, refreshToken);

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:RefreshTokenExpiredMin"]!))
            });
            await _context.SaveChangesAsync();

            return Ok(new { Version = "1.0", Token = jwt, RefreshToken = refreshToken });
        }

        [HttpPost("login")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> LoginV2(LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized();

            var jwt = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            AddRefreshToken(user.Id, refreshToken);

            return Ok(new { Version = "2.0", Token = jwt, RefreshToken = refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var token = await _context.RefreshTokens.Include(r => r.User)
                .SingleOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked);

            if (token == null || token.Expires < DateTime.UtcNow)
                return Unauthorized();

            token.IsRevoked = true;

            var newJwt = GenerateJwtToken(token.User!);
            var newRefreshToken = GenerateRefreshToken();

            AddRefreshToken(token.UserId, newRefreshToken);

            return Ok(new { Token = newJwt, RefreshToken = newRefreshToken });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username is already taken");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        private void AddRefreshToken(int userId, string refreshToken)
        {
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:RefreshTokenExpiredMin"]!))
            });

            _context.SaveChanges();
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenExpiredMin"]!)),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
