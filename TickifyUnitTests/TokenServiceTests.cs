using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Tickify.Services.Authentication;

namespace Tickify.Tests.Services
{
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private TokenService _tokenService;

        [SetUp]
        public void SetUp()
        {
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["Jwt:ValidIssuer"]).Returns("TickifyIssuer");
            _configurationMock.Setup(c => c["Jwt:ValidAudience"]).Returns("TickifyAudience");

            _configurationMock.Setup(c => c["Jwt:IssuerSigningKey"])
                .Returns("this_is_a_super_secure_jwt_signing_key_123456");

            _tokenService = new TokenService(_configurationMock.Object);
        }

        [Test]
        public void CreateToken_ShouldReturnValidJwtToken_WithExpectedClaims()
        {
            var user = new IdentityUser
            {
                Id = "user123",
                UserName = "john_doe",
                Email = "john@example.com"
            };

            var roles = new List<string> { "User", "Admin" };

            var tokenString = _tokenService.CreateToken(user, roles);

            Assert.IsNotNull(tokenString);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            Assert.AreEqual("TickifyIssuer", token.Issuer);
            Assert.AreEqual("TickifyAudience", token.Audiences.First());

            var claims = token.Claims.ToList();
            Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Email && c.Value == user.Email));
            Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Name && c.Value == user.UserName));
            Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id));
            Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "User"));
            Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"));
        }
    }
}
