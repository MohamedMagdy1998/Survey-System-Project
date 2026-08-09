using Domain.Common.Interfaces;
using Domain.Models;
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
    public (string Token, int ExpiresIn) GenerateToken(ApplicationUser user)
    {
        Claim[] claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        };

        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("J7MfAb4WcAIMkkigVtIepIILOVJEjAcB")); // Replace with
    
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);


        
        var expiresIn = 30;

        var token = new JwtSecurityToken(
            issuer: "SurveySystem", 
            audience: "SurveySystem Users",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, ExpiresIn: expiresIn * 60);


    }
}
