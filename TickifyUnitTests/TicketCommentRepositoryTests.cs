using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tickify.Context;
using Tickify.Models;
using Tickify.Repositories;

namespace Tickify.Tests.Repositories
{
    public class TicketCommentRepositoryTests
    {
        private ApplicationDbContext _dbContext;
        private TicketCommentRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new TicketCommentRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetCommentsByTicketIdAsync_ShouldReturnCommentsInOrder()
        {
            var ticketId = 1;
            var comment1 = new TicketComment
            {
                TicketId = ticketId,
                Comment = "First",
                CommenterName = "User",
                CommentedBy = "user1",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            var comment2 = new TicketComment
            {
                TicketId = ticketId,
                Comment = "Second",
                CommenterName = "User",
                CommentedBy = "user1",
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.TicketComments.AddRangeAsync(comment1, comment2);
            await _dbContext.SaveChangesAsync();

            var result = (await _repository.GetCommentsByTicketIdAsync(ticketId)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("First", result[0].Comment);
            Assert.AreEqual("Second", result[1].Comment);
        }

        [Test]
        public async Task AddCommentAsync_ShouldAddToContext()
        {
            var comment = new TicketComment
            {
                TicketId = 1,
                Comment = "New comment",
                CommenterName = "User",
                CommentedBy = "user123",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddCommentAsync(comment);
            await _repository.SaveChangesAsync();

            var saved = await _dbContext.TicketComments.FirstOrDefaultAsync();
            Assert.IsNotNull(saved);
            Assert.AreEqual("New comment", saved.Comment);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            var comment = new TicketComment
            {
                TicketId = 2,
                Comment = "Save test",
                CommenterName = "Tester",
                CommentedBy = "tester1",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddCommentAsync(comment);

            await _repository.SaveChangesAsync();

            var exists = await _dbContext.TicketComments.AnyAsync(c => c.Comment == "Save test");
            Assert.IsTrue(exists);
        }
    }
}
