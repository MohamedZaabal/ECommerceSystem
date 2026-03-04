using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthController(UserManager<IdentityUser> _userManager, IConfiguration _configuration, RoleManager<IdentityRole> _roleManager)
        {
            this._userManager = _userManager;
            this._configuration = _configuration;
            this._roleManager = _roleManager;

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

            return Ok(new { token });
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