using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tickify.Context;
using Tickify.Contracts;
using Tickify.DTOs;
using Tickify.Models;

namespace Tickify.IntegrationTests.Controllers
{
    public class AdminControllerTests
    {
        private HttpClient _client;
        private TickifyWebApplicationFactory _factory;
        private IdentityUser _superAdmin;

        [SetUp]
        public async Task Setup()
        {
            _factory = new TickifyWebApplicationFactory();
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Users.RemoveRange(db.Users);
            db.Tickets.RemoveRange(db.Tickets);
            await db.SaveChangesAsync();

            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

            _superAdmin = new IdentityUser { UserName = "admin", Email = "admin@tickify.com" };
            await userManager.CreateAsync(_superAdmin, "Admin123!");
            await userManager.AddToRoleAsync(_superAdmin, "SuperAdmin");

            var login = new AuthRequest("admin@tickify.com", "Admin123!");
            var response = await _client.PostAsJsonAsync("/Auth/Login", login);
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetUsers_ShouldReturnUserList()
        {
            var response = await _client.GetAsync("/api/admin/users");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var users = await response.Content.ReadFromJsonAsync<List<object>>();
            Assert.That(users, Is.Not.Null);
            Assert.That(users!.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task DashboardStats_ShouldReturnValidCounts()
        {
            var response = await _client.GetAsync("/api/admin/dashboard");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            Assert.That(result, Contains.Key("totalTickets"));
        }

        [Test]
        public async Task AssignToMe_ShouldReturnSuccess()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var ticket = new Ticket
            {
                Title = "Unassigned Ticket",
                Description = "To be assigned",
                Status = "Open",
                CreatedBy = _superAdmin.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync($"/api/admin/tickets/{ticket.Id}/assign-to-me", null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UpdateTicketStatus_ShouldUpdateStatus()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var ticket = new Ticket
            {
                Title = "Status Change Ticket",
                Description = "For testing status update",
                Status = "Open",
                CreatedBy = _superAdmin.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();

            var response = await _client.PutAsync($"/api/admin/tickets/{ticket.Id}/status/Resolved", null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
