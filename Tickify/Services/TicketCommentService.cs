using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tickify.Models;
using Tickify.Repositories;
using Tickify.DTOs;

namespace Tickify.Services
{
    public class TicketCommentService : ITicketCommentService
    {
        private readonly ITicketCommentRepository _commentRepository;
        private readonly ITicketRepository _ticketRepository;

        public TicketCommentService(ITicketCommentRepository commentRepository, ITicketRepository ticketRepository)
        {
            _commentRepository = commentRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<IEnumerable<TicketComment>> GetCommentsByTicketIdAsync(int ticketId)
        {
            return await _commentRepository.GetCommentsByTicketIdAsync(ticketId);
        }

        public async Task AddCommentAsync(int ticketId, string comment, string userId, string? imageUrl)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            var newComment = new TicketComment
            {
                TicketId = ticketId,
                Comment = comment,
                CommentedBy = userId,
                CreatedAt = DateTime.Now,
                ImageUrl = imageUrl 
            };

            await _commentRepository.AddCommentAsync(newComment);
            await _commentRepository.SaveChangesAsync();
        }
    }
}
