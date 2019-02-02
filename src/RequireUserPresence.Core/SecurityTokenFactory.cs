using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RequireUserPresence.Core
{
    public interface ISecurityTokenFactory
    {
        string Create(string tenantId, string userId, string uniqueName);
        string Create(string uniqueName);
    }
    public class SecurityTokenFactory : ISecurityTokenFactory
    {
        public string Create(string uniqueName)
        {
            var claims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, uniqueName),
                    new Claim(JwtRegisteredClaimNames.Sub, uniqueName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "System"),
                    new Claim("UniqueIdentifier",uniqueName)
                };

            return WriteToken(claims);
        }

        public string Create(string tenantId, string userId, string uniqueName)
        {            
            var claims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.UniqueName, uniqueName),
                    new Claim(JwtRegisteredClaimNames.Sub, uniqueName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),                    
                    new Claim("TenantId", tenantId),
                    new Claim("UserId",userId),
                    new Claim("UniqueIdentifier",$"{tenantId}-{uniqueName}")
                };
            
            return WriteToken(claims);
        }

        public string WriteToken(List<Claim> claims)
        {            
            var now = DateTime.UtcNow;
            var nowDateTimeOffset = new DateTimeOffset(now);

            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, nowDateTimeOffset.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(10800),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("UHxNtYMRYwvfpO1dS4pWLKL0M3DgOj30EbN4SoBWgfc")), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
