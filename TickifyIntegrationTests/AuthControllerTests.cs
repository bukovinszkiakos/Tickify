using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;
using Tickify.Contracts;

namespace Tickify.IntegrationTests.Controllers
{
    public class AuthControllerTests
    {
        private HttpClient _client;
        private TickifyWebApplicationFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new TickifyWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Register_Should_Return_Created()
        {
            var request = new RegistrationRequest("authuser@example.com", "authuser", "Test123!");

            var response = await _client.PostAsJsonAsync("/Auth/Register", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var result = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
            Assert.That(result.Email, Is.EqualTo("authuser@example.com"));
            Assert.That(result.Username, Is.EqualTo("authuser"));
        }

        [Test]
        public async Task Login_Should_Return_Token()
        {
            var register = new RegistrationRequest("loginuser@example.com", "loginuser", "Test123!");
            await _client.PostAsJsonAsync("/Auth/Register", register);

            var login = new AuthRequest("loginuser@example.com", "Test123!");
            var response = await _client.PostAsJsonAsync("/Auth/Login", login);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.That(result.Email, Is.EqualTo("loginuser@example.com"));
            Assert.That(result.Username, Is.EqualTo("loginuser"));
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task Me_Should_Return_UserInfo_WhenAuthenticated()
        {
            var register = new RegistrationRequest("meuser@example.com", "meuser", "Test123!");
            await _client.PostAsJsonAsync("/Auth/Register", register);

            var login = new AuthRequest("meuser@example.com", "Test123!");
            var loginResponse = await _client.PostAsJsonAsync("/Auth/Login", login);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            var meResponse = await _client.GetAsync("/Auth/Me");

            Assert.That(meResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var me = await meResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            Assert.That(me["email"].ToString(), Is.EqualTo("meuser@example.com"));
            Assert.That(me["username"].ToString(), Is.EqualTo("meuser"));
            Assert.That(me["roles"].ToString(), Does.Contain("User"));
        }

        [Test]
        public async Task Me_Should_Return_Unauthorized_IfNotAuthenticated()
        {
            var response = await _client.GetAsync("/Auth/Me");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Logout_Should_ClearCookie()
        {
            var response = await _client.PostAsync("/Auth/Logout", null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.That(result["message"], Is.EqualTo("Logout successful"));
        }
    }
}
