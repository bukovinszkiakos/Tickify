using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Tickify.Context;
using Tickify.Models;
using Tickify.Repositories;
using Tickify.Services;

namespace Tickify.Tests.Services
{
    public class TicketCommentServiceTests
    {
        private Mock<ITicketCommentRepository> _commentRepoMock;
        private Mock<ITicketRepository> _ticketRepoMock;
        private ApplicationDbContext _dbContext;
        private TicketCommentService _service;

        [SetUp]
        public void SetUp()
        {
            _commentRepoMock = new Mock<ITicketCommentRepository>();
            _ticketRepoMock = new Mock<ITicketRepository>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _service = new TicketCommentService(_commentRepoMock.Object, _ticketRepoMock.Object, _dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetCommentsByTicketIdAsync_ShouldCallRepository()
        {
            var ticketId = 1;
            var comments = new List<TicketComment> { new TicketComment { Id = 1, TicketId = ticketId } };

            _commentRepoMock.Setup(repo => repo.GetCommentsByTicketIdAsync(ticketId)).ReturnsAsync(comments);

            var result = await _service.GetCommentsByTicketIdAsync(ticketId);

            Assert.AreEqual(1, result.Count());
            _commentRepoMock.Verify(repo => repo.GetCommentsByTicketIdAsync(ticketId), Times.Once);
        }

        [Test]
        public void AddCommentAsync_ShouldThrow_WhenTicketNotFound()
        {
            _ticketRepoMock.Setup(repo => repo.GetTicketByIdAsync(999)).ReturnsAsync((Ticket)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.AddCommentAsync(999, "Test", "1", "John", null));

            Assert.That(ex.Message, Is.EqualTo("Ticket not found."));
        }

        [Test]
        public async Task AddCommentAsync_ShouldAddCommentAndNotification()
        {
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket",
                CreatedBy = "userA"
            };

            _ticketRepoMock.Setup(repo => repo.GetTicketByIdAsync(ticket.Id)).ReturnsAsync(ticket);

            await _dbContext.TicketComments.AddAsync(new TicketComment
            {
                TicketId = ticket.Id,
                CommentedBy = "admin1",
                Comment = "Previous comment",
                CommenterName = "Admin One"
            });

            await _dbContext.TicketHistories.AddAsync(new TicketHistory
            {
                TicketId = ticket.Id,
                ChangedBy = "admin2",
                OldStatus = "Open",     
                NewStatus = "InProgress" 
            });

            await _dbContext.SaveChangesAsync();

            await _service.AddCommentAsync(ticket.Id, "Nice work", "userB", "User B", null);

            _commentRepoMock.Verify(r => r.AddCommentAsync(It.IsAny<TicketComment>()), Times.Once);
            _commentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            var notifications = _dbContext.Notifications.ToList();
            Assert.AreEqual(3, notifications.Count); 

            Assert.IsTrue(notifications.All(n => n.TicketId == ticket.Id.ToString()));
            Assert.IsTrue(notifications.Any(n => n.UserId == "admin1"));
            Assert.IsTrue(notifications.Any(n => n.UserId == "admin2"));
            Assert.IsTrue(notifications.Any(n => n.UserId == "userA"));
        }


    }
}
