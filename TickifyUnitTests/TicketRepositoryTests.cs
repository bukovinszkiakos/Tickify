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
    public class TicketRepositoryTests
    {
        private ApplicationDbContext _dbContext;
        private TicketRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new TicketRepository(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllTicketsAsync_ShouldReturnAllTickets()
        {
            await _dbContext.Tickets.AddRangeAsync(
                new Ticket { Title = "A", Description = "Test1", CreatedBy = "user1" },
                new Ticket { Title = "B", Description = "Test2", CreatedBy = "user2" }
            );
            await _dbContext.SaveChangesAsync();

            var result = (await _repository.GetAllTicketsAsync()).ToList();

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetTicketByIdAsync_ShouldReturnCorrectTicket()
        {
            var ticket = new Ticket { Title = "Important", Description = "Check me", CreatedBy = "user1" };
            await _dbContext.Tickets.AddAsync(ticket);
            await _dbContext.SaveChangesAsync();

            var found = await _repository.GetTicketByIdAsync(ticket.Id);

            Assert.IsNotNull(found);
            Assert.AreEqual("Important", found.Title);
        }

        [Test]
        public async Task AddTicketAsync_ShouldAddTicketToContext()
        {
            var ticket = new Ticket { Title = "New", Description = "Brand new", CreatedBy = "userX" };

            await _repository.AddTicketAsync(ticket);
            await _repository.SaveChangesAsync();

            var exists = await _dbContext.Tickets.AnyAsync(t => t.Title == "New");
            Assert.IsTrue(exists);
        }

        [Test]
        public async Task UpdateTicket_ShouldModifyTicket()
        {
            var ticket = new Ticket { Title = "Original", Description = "To be updated", CreatedBy = "user1" };
            await _dbContext.Tickets.AddAsync(ticket);
            await _dbContext.SaveChangesAsync();

            ticket.Title = "Updated";
            _repository.UpdateTicket(ticket);
            await _repository.SaveChangesAsync();

            var updated = await _dbContext.Tickets.FindAsync(ticket.Id);
            Assert.AreEqual("Updated", updated.Title);
        }

        [Test]
        public async Task DeleteTicket_ShouldRemoveTicket()
        {
            var ticket = new Ticket { Title = "ToDelete", Description = "Bye", CreatedBy = "user2" };
            await _dbContext.Tickets.AddAsync(ticket);
            await _dbContext.SaveChangesAsync();

            _repository.DeleteTicket(ticket);
            await _repository.SaveChangesAsync();

            var exists = await _dbContext.Tickets.AnyAsync(t => t.Id == ticket.Id);
            Assert.IsFalse(exists);
        }

        [Test]
        public async Task GetTicketsByUserAsync_ShouldReturnUserTicketsOnly()
        {
            await _dbContext.Tickets.AddRangeAsync(
                new Ticket { Title = "User1 T1", CreatedBy = "user1", Description = "desc" },
                new Ticket { Title = "User1 T2", CreatedBy = "user1", Description = "desc" },
                new Ticket { Title = "User2 T1", CreatedBy = "user2", Description = "desc" }
            );
            await _dbContext.SaveChangesAsync();

            var result = (await _repository.GetTicketsByUserAsync("user1")).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(t => t.CreatedBy == "user1"));
        }

    }
}
