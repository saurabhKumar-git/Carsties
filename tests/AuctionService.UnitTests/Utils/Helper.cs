using System;
using System.Security.Claims;

namespace AuctionService.UnitTests.Utils;

public class Helper
{
    public static ClaimsPrincipal GetClaimsPrincipal()
    {
        var claims = new List<Claim> { new("userName", "test"), new(ClaimTypes.Name, "test") };
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
