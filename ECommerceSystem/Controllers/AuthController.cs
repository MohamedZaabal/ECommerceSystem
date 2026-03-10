using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerceSystem.Application.Interface;
using ECommerceSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public AuthController(UserManager<IdentityUser> _userManager, IConfiguration _configuration, RoleManager<IdentityRole> _roleManager, IUnitOfWork _unitOfWork)
        {
            this._userManager = _userManager;
            this._configuration = _configuration;
            this._roleManager = _roleManager;
            this._unitOfWork = _unitOfWork;
            Task.Run(async () =>
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                if (!await _roleManager.RoleExistsAsync("Customer"))
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }).GetAwaiter().GetResult();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(String email, string password)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };
            var result = await _userManager.CreateAsync(user, password);
            if(!result.Succeeded)
                return BadRequest(result.Errors);

            if (email.ToLower() == "admin@gmail.com")
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Ok("Admin registered successfully");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return Ok("User registered successfully");
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                return Unauthorized();
            
            var token = GenerateJwtToken(user);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = GenerateRandomToken(),
                Expires = DateTime.UtcNow.AddDays(7)
            };
            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            return Ok(new { token,refreshToken=refreshToken.Token });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            var refreshToken = (await _unitOfWork.RefreshTokens.GetAllAsync())
                                .FirstOrDefault(rt => rt.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                return Unauthorized(new { message = "Invalid refresh token" });

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            var newJwt = await GenerateJwtToken(user);

            return Ok(new
            {
                token = newJwt
            });
        }

        private string GenerateRandomToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var roles=await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
           {
            new Claim(ClaimTypes.NameIdentifier,user.Id),
            new Claim(ClaimTypes.Email, user.Email)

            };
            claims.AddRange(roles.Select(role=>new Claim(ClaimTypes.Role,role)));


            var jwtSettings = _configuration.GetSection("Jwt");
           
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken (
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(
                    double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
                                             );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}