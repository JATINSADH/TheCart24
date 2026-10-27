
using Microsoft.AspNetCore.Identity;

namespace TheCart24.Api.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(IdentityUser user);
}
