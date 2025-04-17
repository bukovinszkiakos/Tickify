using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tickify.Context;
using Tickify.Models;
using Tickify.DTOs;
using Tickify.Contracts;


namespace Tickify.IntegrationTests.Controllers
{
    public class UserControllerTests
    {
        private HttpClient _client;
        private TickifyWebApplicationFactory _factory;

        [SetUp]
        public async Task Setup()
        {
            _factory = new TickifyWebApplicationFactory();
            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            db.Notifications.RemoveRange(db.Notifications);
            db.Users.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            var register = new RegistrationRequest("test@example.com", "testuser", "Test123!");
            await _client.PostAsJsonAsync("/Auth/Register", register);

            var loginDto = new AuthRequest("test@example.com", "Test123!");
            var response = await _client.PostAsJsonAsync("/Auth/Login", loginDto);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);

            var user = await userManager.FindByEmailAsync("test@example.com");

            db.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Message = "Test notification",
                TicketId = null,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                CreatedBy = null
            });
            await db.SaveChangesAsync();
        }


        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }


        [Test]
        public async Task GetNotifications_ShouldReturnNotifications_ForAuthorizedUser()
        {
            var response = await _client.GetAsync("/api/User/notifications");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var data = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();

            Assert.IsNotNull(data);
            Assert.That(data, Has.Count.EqualTo(1));
            Assert.That(data[0].Message, Is.EqualTo("Test notification"));
        }

        [Test]
        public async Task MarkAsRead_ShouldUpdateNotificationStatus()
        {
            using var scope1 = _factory.Services.CreateScope();
            var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notification = db1.Notifications.First();

            var response = await _client.PostAsync($"/api/User/notifications/{notification.Id}/read", null);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope2 = _factory.Services.CreateScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updated = await db2.Notifications.FindAsync(notification.Id);

            Assert.That(updated!.IsRead, Is.True);
        }

        [Test]
        public async Task DeleteNotification_ShouldRemoveNotification()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = await userManager.FindByEmailAsync("test@example.com");

            var notification = new Notification
            {
                UserId = user.Id,
                Message = "To be deleted",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            var response = await _client.DeleteAsync($"/api/User/notifications/{notification.Id}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var scope2 = _factory.Services.CreateScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deleted = await db2.Notifications.FindAsync(notification.Id);

            Assert.That(deleted, Is.Null);
        }




    }
}
