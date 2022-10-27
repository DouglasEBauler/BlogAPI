using Blog.Models;
using System.Security.Claims;

namespace BlogAPI.Extensions;

public static class RoleClaimsExtensions
{
    public static IEnumerable<Claim> GetClaims(this User user)
    {
        var result = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.Email),
        };

        result.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug)));

        return result;
    }
}
