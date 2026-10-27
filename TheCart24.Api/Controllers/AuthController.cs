
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheCart24.Api.DTOs;
using TheCart24.Api.Services;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var existingUser = await userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return BadRequest(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(error => error.Description)
            });
        }

        var roleResult = await userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return StatusCode(500, new
            {
                message = "Could not assign the default user role."
            });
        }

        return Ok(new
        {
            message = "Registration successful."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user is null ||
            !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var token = await tokenService.CreateTokenAsync(user);

        return Ok(new
        {
            message = "Login successful.",
            token,
            expiresInMinutes = 30
        });
    }
}
