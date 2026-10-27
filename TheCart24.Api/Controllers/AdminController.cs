
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(
    UserManager<IdentityUser> userManager) : ControllerBase
{


    [HttpPost("promote/{userId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PromoteToAdmin(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        var existingRoles = await userManager.GetRolesAsync(user);

        // SuperAdmin ko change nahi karna hai.
        if (existingRoles.Contains("SuperAdmin"))
        {
            return BadRequest(new
            {
                message = "SuperAdmin cannot be changed through this endpoint."
            });
        }

        // Agar sirf Admin role hai, toh kuch change nahi karna.
        if (existingRoles.Count == 1 &&
            existingRoles.Contains("Admin"))
        {
            return Ok(new
            {
                message = "User is already an Admin."
            });
        }

        // User ke saare existing roles remove karo.
        if (existingRoles.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(
                user, existingRoles);

            if (!removeResult.Succeeded)
            {
                return BadRequest(new
                {
                    errors = removeResult.Errors.Select(
                        e => e.Description)
                });
            }
        }

        // Ab sirf Admin role assign karo.
        var addResult = await userManager.AddToRoleAsync(
            user, "Admin");

        if (!addResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Old roles were removed, but Admin assignment failed.",
                errors = addResult.Errors.Select(
                    e => e.Description)
            });
        }

        return Ok(new
        {
            message = "User role changed to Admin successfully.",
            userId = user.Id,
            email = user.Email
        });
    }


    [HttpGet("dashboard")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Dashboard()
    {
        return Ok(new
        {
            message = "Admin dashboard access granted.",
            email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email)?.Value,
            roles = User.FindAll(
                System.Security.Claims.ClaimTypes.Role)
                .Select(role => role.Value)
        });
    }
}
