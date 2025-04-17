using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tickify.Context;
using Tickify.Models;
using Tickify.Contracts;

namespace Tickify.IntegrationTests.Controllers
{
    public class TicketCommentsControllerTests
    {
        private HttpClient _client;
        private TickifyWebApplicationFactory _factory;
        private IdentityUser _testUser;
        private int _ticketId;

        [SetUp]
        public async Task Setup()
        {
            _factory = new TickifyWebApplicationFactory();
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            db.Tickets.RemoveRange(db.Tickets);
            db.TicketComments.RemoveRange(db.TicketComments);
            db.CommentReadStatuses.RemoveRange(db.CommentReadStatuses);
            db.Users.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            var register = new RegistrationRequest("comment@example.com", "commenter", "Test123!");
            await _client.PostAsJsonAsync("/Auth/Register", register);

            var login = new AuthRequest("comment@example.com", "Test123!");
            var loginResponse = await _client.PostAsJsonAsync("/Auth/Login", login);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
            _testUser = await userManager.FindByEmailAsync("comment@example.com");

            var ticket = new Ticket
            {
                Title = "Test Ticket",
                Description = "For comment tests",
                CreatedBy = _testUser.Id,
                Priority = "Normal",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            _ticketId = ticket.Id;
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetComments_ShouldReturnComments_ForExistingTicket()
        {
            var commentForm = new MultipartFormDataContent();
            commentForm.Add(new StringContent("This is a test comment"), "comment");

            var postResponse = await _client.PostAsync($"/api/tickets/{_ticketId}/comments", commentForm);
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var getResponse = await _client.GetAsync($"/api/tickets/{_ticketId}/comments");
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await getResponse.Content.ReadFromJsonAsync<List<object>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AddComment_ShouldAddCommentSuccessfully()
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent("This is another test comment"), "comment" }
            };

            var response = await _client.PostAsync($"/api/tickets/{_ticketId}/comments", form);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddComment_ShouldReturnNotFound_ForInvalidTicket()
        {
            var form = new MultipartFormDataContent
            {
                { new StringContent("Invalid ticket comment"), "comment" }
            };

            var response = await _client.PostAsync("/api/tickets/999999/comments", form);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
