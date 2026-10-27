
using Microsoft.AspNetCore.Identity;

namespace TheCart24.Api.Data;

public static class SuperAdminSeeder
{
    public static async Task PromoteSuperAdminAsync(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SeedSuperAdmin:Email"];

        if (string.IsNullOrWhiteSpace(email))
            return;

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Configured SuperAdmin account was not found. Register it first.");
        }

        if (!await userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            var result = await userManager.AddToRoleAsync(
                user, "SuperAdmin");

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"SuperAdmin assignment failed: {errors}");
            }
        }
    }
}
