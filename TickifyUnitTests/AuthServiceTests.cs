using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Tickify.Contracts;
using Tickify.Services.Authentication;

namespace Tickify.Tests.Services
{
    public class AuthServiceTests
    {
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private AuthService _authService;

        [SetUp]
        public void SetUp()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );
            _tokenServiceMock = new Mock<ITokenService>();

            _authService = new AuthService(_userManagerMock.Object, _tokenServiceMock.Object);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsCreated()
        {
            var email = "test@example.com";
            var username = "testuser";
            var password = "Test@123";

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _authService.RegisterAsync(email, username, password);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(username, result.Username);
            Assert.IsEmpty(result.ErrorMessages);
        }

        [Test]
        public async Task RegisterAsync_ShouldReturnFailure_WhenCreateFails()
        {
            var email = "test@example.com";
            var username = "testuser";
            var password = "Test@123";

            var identityResult = IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" });

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(identityResult);

            var result = await _authService.RegisterAsync(email, username, password);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessages.ContainsKey("DuplicateEmail"));
        }

        [Test]
        public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            var email = "test@example.com";
            var password = "Test@123";
            var user = new IdentityUser { UserName = "testuser", Email = email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.CreateToken(user, It.IsAny<List<string>>())).Returns("mocked-token");

            var result = await _authService.LoginAsync(email, password);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("mocked-token", result.Token);
            Assert.AreEqual(user.Email, result.Email);
        }

        [Test]
        public async Task LoginAsync_ShouldFail_WhenUserNotFound()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

            var result = await _authService.LoginAsync("fake@example.com", "pass");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessages.ContainsKey("BadCredentials"));
        }

        [Test]
        public async Task LoginAsync_ShouldFail_WhenPasswordInvalid()
        {
            var user = new IdentityUser { Email = "test@example.com", UserName = "testuser" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "wrongpass")).ReturnsAsync(false);

            var result = await _authService.LoginAsync(user.Email, "wrongpass");

            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessages.ContainsKey("BadCredentials"));
        }
    }
}
