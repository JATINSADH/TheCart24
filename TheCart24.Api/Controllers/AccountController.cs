
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TheCart24.Api.Controllers;

[ApiController]
[Route("api/Account")]
[Authorize]
public class AccountController(
    UserManager<IdentityUser> userManager) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyAccount()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            fullName = user.UserName ?? "",
            email = user.Email ?? "",
            phoneNumber = user.PhoneNumber ?? ""
        });
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Account could not be deleted.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message = "Account deleted successfully."
        });
    }
}
