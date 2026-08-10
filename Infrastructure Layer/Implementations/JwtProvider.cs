using Application.Options;
using Domain.Common.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
    public (string Token, int ExpiresIn) GenerateToken(ApplicationUser user)
    {
        Claim[] claims = new []
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        };

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
    
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);


        
        var expiresIn = 30;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer, 
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryInMinutes),
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, ExpiresIn: expiresIn * 60);


    }

    public string? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var symmetricSecurityKey = new
       SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        try
        {
            tokenHandler.ValidateToken(token, new
           TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = symmetricSecurityKey,

                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero // don't add extra time to the token's expiration
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type ==
           JwtRegisteredClaimNames.Sub).Value;
            return userId;
        }
        catch
        {
            return null;
        }

    }

}
