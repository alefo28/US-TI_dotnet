using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BankAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginService service;
    private IConfiguration config;

    public LoginController(LoginService service, IConfiguration config)
    {
        this.service = service;
        this.config = config;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Login(AdminDto adminDto)
    {
        var admin = await service.getAdmin(adminDto);

        if (admin is null)
        {
            return BadRequest(new { message = "Credenciales Inválidas." });
        }

        //Generar token
        string jwtToken = GenerateToken(admin);

        return Ok(new { token = jwtToken });
    }

    private string GenerateToken(Administrator admin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim("AdminType", admin.AdminType)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.GetSection("JWT:key").Value)
        );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds
        );

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }
}
