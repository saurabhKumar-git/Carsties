﻿using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp","Auction app full access"),
        };

    public static IEnumerable<Client> Clients =>
        [
            new Client{
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = {"openid","profile","auctionApp"},
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                ClientSecrets = [new Secret("NotASecret".Sha256())],
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            },
            new Client{
                ClientId = "nextApp",
                ClientName = "NextApp",
                AllowedScopes = {"openid","profile","auctionApp"},
                RequirePkce = false,
                ClientSecrets = [new Secret("NotASecret".Sha256())],
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RedirectUris = {"https://localhost:3000/api/auth/callback/id-server"},
                AllowOfflineAccess = true,
                AccessTokenLifetime = 3600*24*30
            }
        ];
}
