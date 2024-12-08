using System;
using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;

public class AuthHelper
{
    public static Dictionary<string, object> GetBearerForUser(string name)
    {
        return new Dictionary<string, object> { { ClaimTypes.Name, name } };
    }
}
