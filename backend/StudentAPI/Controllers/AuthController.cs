using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StudentAPI.Data;       
using StudentAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentAPI.Services;
using Microsoft.EntityFrameworkCore;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _dbContext;
    private readonly EmailService _emailService;
    private static Dictionary<string, string> otpStorage = new();
    public AuthController(IConfiguration config, ApplicationDbContext dbContext, EmailService emailService)
    {
        _config = config;
        _dbContext = dbContext;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] Registers request)
    {
        if (_dbContext.Registers.Any(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "User already exists" });
        }

        var newUser = new Registers
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = request.PasswordHash 
        };

        _dbContext.Registers.Add(newUser);
        _dbContext.SaveChanges();

        return Ok(new { message = "User registered successfully" });
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _dbContext.Registers.FirstOrDefault(u => u.Email == request.Email && u.PasswordHash == request.PasswordHash);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateJwtToken(user.Email,user.Name);
        return Ok(new { token });
    }
   


    private string GenerateJwtToken(string email, string name)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name) 
        };

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordmodel model)
    {
        if (string.IsNullOrEmpty(model.Email))
            return BadRequest(new { message = "Email is required" });

        // Simulate checking if email exists
        var user = await _dbContext.Registers.FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());

        if (user == null)
            return NotFound(new { message = "User not found" });
        // Generate OTP
        string otp = new Random().Next(100000, 999999).ToString();
        otpStorage[model.Email] = otp;

        // Send OTP via email
        string subject = "Your OTP for Password Reset";
        string message = $"Your OTP is: {otp}. It will expire in 5 minutes.";

        bool emailSent = await _emailService.SendEmailAsync(model.Email, subject, message);

        if (!emailSent)
          
        return StatusCode(500, new { message = "Failed to send OTP. Try again later." });

        return Ok(new { message = "OTP sent to your email." });
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpModel model)
    {
        if (!otpStorage.ContainsKey(model.Email) || otpStorage[model.Email] != model.Otp)
            return BadRequest(new { message = "Invalid OTP" });

        // Clear OTP after successful verification
        otpStorage.Remove(model.Email);
        return Ok(new { message = "OTP verified. User logged in." });
    }

}



