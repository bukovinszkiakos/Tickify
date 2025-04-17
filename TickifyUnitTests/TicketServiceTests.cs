using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Tickify.Context;
using Tickify.DTOs;
using Tickify.Models;
using Tickify.Repositories;
using Tickify.Services;

namespace Tickify.Tests.Services
{
    public class TicketServiceTests
    {
        private Mock<ITicketRepository> _ticketRepoMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<ITicketCommentService> _commentServiceMock;
        private ApplicationDbContext _dbContext;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private TicketService _ticketService;

        [SetUp]
        public void SetUp()
        {
            _ticketRepoMock = new Mock<ITicketRepository>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object, null, null, null, null, null, null, null, null);
            _commentServiceMock = new Mock<ITicketCommentService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _ticketService = new TicketService(
                _ticketRepoMock.Object,
                _userManagerMock.Object,
                _commentServiceMock.Object,
                _dbContext,
                _httpContextAccessorMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public void GetTicketDtoByIdAsync_ShouldThrow_WhenUnauthorized()
        {
            var ticket = new Ticket
            {
                Id = 1,
                CreatedBy = "user123",
                AssignedTo = "admin123"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var currentUser = new IdentityUser { Id = "not-allowed" };

            _userManagerMock.Setup(m => m.FindByIdAsync("not-allowed"))
                .ReturnsAsync(currentUser);

            _userManagerMock.Setup(m => m.IsInRoleAsync(currentUser, "SuperAdmin"))
                .ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _ticketService.GetTicketDtoByIdAsync(1, "not-allowed", isAdmin: false));

            Assert.That(ex.Message, Is.EqualTo("Not allowed to access this ticket."));
        }

        [Test]
        public async Task GetTicketDtoByIdAsync_ShouldReturnTicket_WhenUserIsCreator()
        {
            var ticket = new Ticket
            {
                Id = 1,
                CreatedBy = "user123",
                Title = "Test Ticket"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            await _dbContext.Users.AddAsync(new IdentityUser
            {
                Id = "user123",
                UserName = "creator"
            });
            await _dbContext.SaveChangesAsync();

            var result = await _ticketService.GetTicketDtoByIdAsync(1, "user123", isAdmin: false);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Ticket", result.Title);
            Assert.AreEqual("creator", result.CreatedByName);
        }

        [Test]
        public async Task UpdateTicketAsync_ShouldAddComment_WhenTitleChanged()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Old Title",
                Description = "Same",
                Priority = "Low",
                CreatedBy = "user123",
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var updateDto = new UpdateTicketDto
            {
                Title = "New Title",
                Description = "Same",
                Priority = "Low",
                AssignedTo = null
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("user123"))
                .ReturnsAsync(new IdentityUser { Id = "user123", UserName = "User123" });

            await _ticketService.UpdateTicketAsync(
                1,
                updateDto,
                "user123",
                isAdmin: false,
                image: null,
                scheme: "http",
                host: "localhost"
            );

            _ticketRepoMock.Verify(r => r.UpdateTicket(It.Is<Ticket>(t =>
                t.Title == "New Title" && t.UpdatedAt > DateTime.UtcNow.AddMinutes(-1))), Times.Once);

            _commentServiceMock.Verify(c => c.AddCommentAsync(
                1,
                It.Is<string>(msg => msg.Contains("Title:")),
                "user123",
                "User123",
                null
            ), Times.Once);

            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateTicketAsync_ShouldHandleImageUpload()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Title",
                Description = "Desc",
                Priority = "Low",
                CreatedBy = "user123",
                ImageUrl = "/uploads/old_image.png"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var updateDto = new UpdateTicketDto
            {
                Title = "Title",
                Description = "Desc",
                Priority = "Low",
                AssignedTo = null
            };

            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "test-image.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns<Stream, CancellationToken>((stream, token) => ms.CopyToAsync(stream, token));

            _userManagerMock.Setup(m => m.FindByIdAsync("user123"))
                .ReturnsAsync(new IdentityUser { Id = "user123", UserName = "User123" });

            await _ticketService.UpdateTicketAsync(
                1,
                updateDto,
                "user123",
                isAdmin: false,
                image: fileMock.Object,
                scheme: "http",
                host: "localhost"
            );

            _ticketRepoMock.Verify(r => r.UpdateTicket(It.Is<Ticket>(t =>
                t.ImageUrl != null && t.ImageUrl != "/uploads/old_image.png")), Times.Once);

            _commentServiceMock.Verify(c => c.AddCommentAsync(
                1,
                It.Is<string>(msg =>
                    msg.Contains("🖼️ Image updated.") &&
                    msg.Contains("Old image:") &&
                    msg.Contains("New image:")
                ),
                "user123",
                "User123",
                null
            ), Times.Once);

            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateTicketAsync_ShouldCreateComment_WhenImagePresent()
        {
            var fileMock = new Mock<IFormFile>();
            var content = "fake image";
            var fileName = "image.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns<Stream, CancellationToken>((stream, token) => ms.CopyToAsync(stream, token));

            var userId = "user123";
            var user = new IdentityUser { Id = userId, UserName = "testuser" };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            var result = await _ticketService.CreateTicketAsync(
                title: "Bug",
                description: "Something is broken",
                priority: "High",
                userId: userId,
                isAdmin: false,
                image: fileMock.Object,
                scheme: "http",
                host: "localhost"
            );

            Assert.IsNotNull(result);
            Assert.AreEqual("Bug", result.Title);
            Assert.IsNotNull(result.ImageUrl);

            _commentServiceMock.Verify(c => c.AddCommentAsync(
                It.IsAny<int>(),
                It.Is<string>(msg => msg.StartsWith("Ticket created with image:")),
                userId,
                "testuser",
                null
            ), Times.Once);

            _ticketRepoMock.Verify(r => r.AddTicketAsync(It.IsAny<Ticket>()), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteTicketAsync_ShouldCreateNotification_ForUser()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Old Ticket",
                CreatedBy = "user456",
                AssignedTo = "admin123"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);
            _userManagerMock.Setup(m => m.FindByIdAsync("admin123"))
                .ReturnsAsync(new IdentityUser { Id = "admin123" });

            _userManagerMock.Setup(m => m.IsInRoleAsync(It.IsAny<IdentityUser>(), "SuperAdmin"))
                .ReturnsAsync(false);

            await _ticketService.DeleteTicketAsync(1, "admin123", isAdmin: true);

            _ticketRepoMock.Verify(r => r.DeleteTicket(ticket), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var notifications = _dbContext.Notifications.ToList();

            Assert.AreEqual(1, notifications.Count);
            var notif = notifications[0];
            Assert.AreEqual("user456", notif.UserId);
            Assert.AreEqual("admin123", notif.CreatedBy);
            Assert.IsTrue(notif.Message.Contains("❌ Your ticket"));
            Assert.IsNull(notif.TicketId);
        }


        [Test]
        public async Task GetTicketsForUserAsync_ShouldFilterTickets_ForUser()
        {
            var userId = "user123";

            var tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    Title = "T1",
                    Description = "desc",
                    CreatedBy = "user123",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
            },
                new Ticket
                {
                    Id = 2,
                    Title = "T2",
                    Description = "desc",
                    CreatedBy = "otherUser",
                    Status = "Closed",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

        var comments = new List<TicketComment>
        {
        new TicketComment { Id = 1, TicketId = 1, CommentedBy = "admin", Comment = "Hi", CommenterName = "Admin" },
        new TicketComment { Id = 2, TicketId = 1, CommentedBy = "admin", Comment = "More info?", CommenterName = "Admin" }
        };

        var readStatuses = new List<CommentReadStatus>
        {
        new CommentReadStatus { CommentId = 1, UserId = userId } 
        };

            await _dbContext.Tickets.AddRangeAsync(tickets);
            await _dbContext.TicketComments.AddRangeAsync(comments);
            await _dbContext.CommentReadStatuses.AddRangeAsync(readStatuses);
            await _dbContext.Users.AddAsync(new IdentityUser { Id = "user123", UserName = "TestUser" });
            await _dbContext.Users.AddAsync(new IdentityUser { Id = "admin", UserName = "Admin" });
            await _dbContext.SaveChangesAsync();

            _ticketRepoMock.Setup(r => r.GetAllTicketsAsync()).ReturnsAsync(tickets);

            var result = (await _ticketService.GetTicketsForUserAsync(userId, isAdmin: false)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual(2, result[0].TotalCommentCount);
            Assert.AreEqual(1, result[0].UnreadCommentCount);
        }

        [Test]
        public async Task GetTicketsForUserAsync_ShouldReturnAll_WhenAdmin()
        {
            var adminId = "admin123";

            var tickets = new List<Ticket>
            {
                 new Ticket
                 {
                    Id = 1,
                    Title = "T1",
                    Description = "desc",
                    CreatedBy = "user1",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                 },
                
                new Ticket
                {
                    Id = 2,
                    Title = "T2",
                    Description = "desc",
                    CreatedBy = "user2",
                    Status = "Closed",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                 }
            };

        var comments = new List<TicketComment>
        {
        new TicketComment { Id = 1, TicketId = 1, CommentedBy = "user1", Comment = "Hi", CommenterName = "User1" },
        new TicketComment { Id = 2, TicketId = 2, CommentedBy = "admin123", Comment = "Checked", CommenterName = "Admin" }
        };

        var readStatuses = new List<CommentReadStatus>
        {
        new CommentReadStatus { CommentId = 1, UserId = adminId } 
         };

            await _dbContext.Tickets.AddRangeAsync(tickets);
            await _dbContext.TicketComments.AddRangeAsync(comments);
            await _dbContext.CommentReadStatuses.AddRangeAsync(readStatuses);
            await _dbContext.Users.AddAsync(new IdentityUser { Id = "user1", UserName = "User1" });
            await _dbContext.Users.AddAsync(new IdentityUser { Id = "user2", UserName = "User2" });
            await _dbContext.Users.AddAsync(new IdentityUser { Id = "admin123", UserName = "Admin" });
            await _dbContext.SaveChangesAsync();

            _ticketRepoMock.Setup(r => r.GetAllTicketsAsync()).ReturnsAsync(tickets);

            var result = (await _ticketService.GetTicketsForUserAsync(adminId, isAdmin: true)).ToList();

            Assert.AreEqual(2, result.Count); 

            var ticket1 = result.First(t => t.Id == 1);
            var ticket2 = result.First(t => t.Id == 2);

            Assert.AreEqual(1, ticket1.TotalCommentCount);
            Assert.AreEqual(0, ticket1.UnreadCommentCount); 

            Assert.AreEqual(1, ticket2.TotalCommentCount);
            Assert.AreEqual(0, ticket2.UnreadCommentCount); 
        }


        [Test]
        public async Task GetTicketDtoByIdAsync_ShouldReturnTicket_WhenAdminAssigned()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket",
                Description = "Some issue",
                CreatedBy = "user123",
                AssignedTo = "admin123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var admin = new IdentityUser { Id = "admin123", UserName = "AdminUser" };
            await _dbContext.Users.AddAsync(admin);
            await _dbContext.SaveChangesAsync();

            _userManagerMock.Setup(m => m.FindByIdAsync("admin123")).ReturnsAsync(admin);
            _userManagerMock.Setup(m => m.IsInRoleAsync(admin, "SuperAdmin")).ReturnsAsync(false);

            var result = await _ticketService.GetTicketDtoByIdAsync(1, "admin123", isAdmin: true);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Ticket", result.Title);
            Assert.AreEqual("admin123", result.AssignedTo);
            Assert.AreEqual("AdminUser", result.AssignedToName);
        }

        [Test]
        public void GetTicketDtoByIdAsync_ShouldThrow_WhenNotAdminNorCreatorNorAssigned()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Private Ticket",
                Description = "Sensitive",
                CreatedBy = "user456",
                AssignedTo = "admin789"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var outsider = new IdentityUser { Id = "intruder" };
            _userManagerMock.Setup(m => m.FindByIdAsync("intruder")).ReturnsAsync(outsider);
            _userManagerMock.Setup(m => m.IsInRoleAsync(outsider, "SuperAdmin")).ReturnsAsync(false);

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _ticketService.GetTicketDtoByIdAsync(1, "intruder", isAdmin: false));

            Assert.That(ex.Message, Is.EqualTo("Not allowed to access this ticket."));
        }

        [Test]
        public async Task MarkTicketCommentsAsReadAsync_ShouldAddStatuses_ForUnreadComments()
        {
            var userId = "user123";

            var ticket = new Ticket
            {
                Id = 1,
                Title = "Bug",
                Description = "Issue",
                CreatedBy = userId,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Tickets.AddAsync(ticket);

            await _dbContext.TicketComments.AddRangeAsync(new List<TicketComment>
            {
                new TicketComment
                {
                    Id = 1,
                    TicketId = 1,
                    CommentedBy = "admin456",
                    Comment = "Please check this",
                    CommenterName = "Admin"
                  },

                new TicketComment
                 {
                    Id = 2,
                    TicketId = 1,
                    CommentedBy = "admin456",
                    Comment = "Still waiting",
                    CommenterName = "Admin"
                },
       
                new TicketComment
                {
                    Id = 3,
                    TicketId = 1,
                    CommentedBy = userId, 
                    Comment = "I'm on it",
                    CommenterName = "User123"
                }
             });

            await _dbContext.CommentReadStatuses.AddAsync(new CommentReadStatus
            {
                CommentId = 1,
                UserId = userId,
                SeenAt = DateTime.UtcNow.AddDays(-1)
            });

            await _dbContext.SaveChangesAsync();

            await _ticketService.MarkTicketCommentsAsReadAsync(1, userId);

            var readStatuses = await _dbContext.CommentReadStatuses
                .Where(r => r.UserId == userId)
                .ToListAsync();

            Assert.AreEqual(2, readStatuses.Count); 
            Assert.IsTrue(readStatuses.Any(r => r.CommentId == 2));
            Assert.IsFalse(readStatuses.Any(r => r.CommentId == 3)); 
        }


        [Test]
        public async Task AssignTicketToAdminAsync_ShouldAssignAndUpdate()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Crash Bug",
                Description = "App crashes on login",
                CreatedBy = "user123",
                Status = "Open",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                AssignedTo = null
            };

            await _dbContext.Tickets.AddAsync(ticket);
            await _dbContext.SaveChangesAsync();

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var beforeUpdate = DateTime.UtcNow;
            await _ticketService.AssignTicketToAdminAsync(1, "admin456");

            _ticketRepoMock.Verify(r => r.UpdateTicket(It.Is<Ticket>(t =>
                t.AssignedTo == "admin456" && t.UpdatedAt > beforeUpdate)), Times.Once);

            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task ReassignTicketAsync_ShouldAllowSuperAdminToReassign()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Old Ticket",
                AssignedTo = "admin123"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var superAdminUser = new IdentityUser { Id = "superadmin" };
            _userManagerMock.Setup(m => m.FindByIdAsync("superadmin")).ReturnsAsync(superAdminUser);
            _userManagerMock.Setup(m => m.IsInRoleAsync(superAdminUser, "SuperAdmin")).ReturnsAsync(true);

            await _ticketService.ReassignTicketAsync(1, "admin999", "superadmin");

            _ticketRepoMock.Verify(r => r.UpdateTicket(It.Is<Ticket>(t =>
                t.AssignedTo == "admin999")), Times.Once);

            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteTicketImageAsync_ShouldReturnFalse_WhenNoImage()
        {
            var ticket = new Ticket
            {
                Id = 1,
                CreatedBy = "user123",
                ImageUrl = null
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var result = await _ticketService.DeleteTicketImageAsync(1, "user123", isAdmin: false);

            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteTicketImageAsync_ShouldThrow_WhenUnauthorizedUser()
        {
            var ticket = new Ticket
            {
                Id = 1,
                CreatedBy = "someoneElse",
                ImageUrl = "/uploads/image.png"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _ticketService.DeleteTicketImageAsync(1, "user123", isAdmin: false));

            Assert.That(ex.Message, Contains.Substring("Not allowed to delete"));
        }


        [Test]
        public async Task DeleteTicketImageAsync_ShouldDeleteFile_WhenExists()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/test_delete.png");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "dummy content");

            var ticket = new Ticket
            {
                Id = 1,
                CreatedBy = "user123",
                ImageUrl = "/uploads/test_delete.png"
            };

            _ticketRepoMock.Setup(r => r.GetTicketByIdAsync(1)).ReturnsAsync(ticket);

            var result = await _ticketService.DeleteTicketImageAsync(1, "user123", isAdmin: false);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(filePath)); 
            _ticketRepoMock.Verify(r => r.UpdateTicket(It.Is<Ticket>(t => t.ImageUrl == null)), Times.Once);
            _ticketRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


    




    }

}



