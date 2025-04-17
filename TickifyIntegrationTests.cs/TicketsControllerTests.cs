using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tickify.Context;
using Tickify.DTOs;
using Tickify.Models;
using Tickify.Contracts;

namespace Tickify.IntegrationTests.Controllers
{
    public class TicketsControllerTests
    {
        private HttpClient _client;
        private TickifyWebApplicationFactory _factory;
        private IdentityUser _testUser;

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
            db.Notifications.RemoveRange(db.Notifications);
            db.TicketComments.RemoveRange(db.TicketComments);
            db.CommentReadStatuses.RemoveRange(db.CommentReadStatuses);
            await db.SaveChangesAsync();

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            var registerDto = new RegistrationRequest("test@example.com", "testuser", "Test123!");
            await _client.PostAsJsonAsync("/Auth/Register", registerDto);

            var loginDto = new AuthRequest("test@example.com", "Test123!");
            var response = await _client.PostAsJsonAsync("/Auth/Login", loginDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

            _testUser = await userManager.FindByEmailAsync("test@example.com");
        }



        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetTickets_ShouldReturnEmptyInitially()
        {
            var response = await _client.GetAsync("/api/Tickets");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var data = await response.Content.ReadFromJsonAsync<List<TicketDto>>();
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CreateTicket_ShouldReturnCreatedTicket()
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("Test Title"), "title");
            formData.Add(new StringContent("Test Description"), "description");
            formData.Add(new StringContent("High"), "priority");

            var response = await _client.PostAsync("/api/Tickets", formData);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var created = await response.Content.ReadFromJsonAsync<TicketDto>();
            Assert.That(created.Title, Is.EqualTo("Test Title"));
        }

        [Test]
        public async Task GetTicket_ShouldReturnCorrectTicket()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ticket = new Ticket
            {
                Title = "Fetched",
                Description = "Details",
                CreatedBy = _testUser.Id,
                Priority = "Medium",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();

            var response = await _client.GetAsync($"/api/Tickets/{ticket.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var dto = await response.Content.ReadFromJsonAsync<TicketDto>();
            Assert.That(dto.Title, Is.EqualTo("Fetched"));
        }

        [Test]
        public async Task UpdateTicket_ShouldModifyTicket()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var ticket = new Ticket
            {
                Title = "Before Update",
                Description = "Initial",
                CreatedBy = _testUser.Id,
                Priority = "Low",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();

            var updateForm = new MultipartFormDataContent
    {
        { new StringContent("Updated Title"), "Title" },
        { new StringContent("Updated Description"), "Description" },
        { new StringContent("High"), "Priority" },
        { new StringContent("Open"), "Status" },
        { new StringContent(""), "AssignedTo" }
    };

            var response = await _client.PutAsync($"/api/Tickets/{ticket.Id}", updateForm);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }


        [Test]
        public async Task DeleteTicket_ShouldRemoveTicket()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ticket = new Ticket
            {
                Title = "To Be Deleted",
                Description = "Soon gone",
                CreatedBy = _testUser.Id,
                Priority = "Medium",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();

            var response = await _client.DeleteAsync($"/api/Tickets/{ticket.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DeleteTicketImage_ShouldReturnNotFound_WhenNoImage()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ticket = new Ticket
            {
                Title = "No Image",
                Description = "Test",
                CreatedBy = _testUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();

            var response = await _client.DeleteAsync($"/api/Tickets/{ticket.Id}/image");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
