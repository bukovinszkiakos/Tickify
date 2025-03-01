using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tickify.DTOs;
using Tickify.Models;
using Tickify.Repositories;

namespace Tickify.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }
        public async Task<IEnumerable<TicketDto>> GetTicketsForUserAsync(string userId, bool isAdmin)
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();

            var ticketDtos = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Status = t.Status,
                Priority = t.Priority,
                CreatedBy = t.CreatedBy,
                AssignedTo = t.AssignedTo
            });

            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                ticketDtos = ticketDtos.Where(t => t.CreatedBy == userId);
            }

            return ticketDtos;
        }

        public async Task<TicketDto> GetTicketDtoByIdAsync(int id, string userId, bool isAdmin)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            if (!isAdmin && ticket.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Not allowed to access this ticket.");
            }

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedBy = ticket.CreatedBy,
                AssignedTo = ticket.AssignedTo,
                ImageUrl = ticket.ImageUrl 
            };
        }


        public async Task<TicketDto> CreateTicketAsync(string title, string description, string priority, string userId, bool isAdmin, IFormFile? image)
        {
            if (isAdmin)
            {
                throw new UnauthorizedAccessException("Admin users are not allowed to create tickets.");
            }

            string? imageUrl = null;

            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}"; 
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrl = $"/uploads/{fileName}"; 
            }

            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                Priority = priority,
                Status = "Open",
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ImageUrl = imageUrl 
            };

            await _ticketRepository.AddTicketAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedBy = ticket.CreatedBy,
                AssignedTo = ticket.AssignedTo,
                ImageUrl = ticket.ImageUrl 
            };
        }



        public async Task UpdateTicketAsync(int id, UpdateTicketDto updateDto, string userId, bool isAdmin)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            if (!isAdmin && ticket.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Not allowed to update this ticket.");
            }

            ticket.Title = updateDto.Title;
            ticket.Description = updateDto.Description;
            ticket.Status = updateDto.Status;
            ticket.Priority = updateDto.Priority;
            ticket.AssignedTo = updateDto.AssignedTo;
            ticket.UpdatedAt = DateTime.Now;

            _ticketRepository.UpdateTicket(ticket);
            await _ticketRepository.SaveChangesAsync();
        }

        public async Task DeleteTicketAsync(int id, string userId, bool isAdmin)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            if (!isAdmin && ticket.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Not allowed to delete this ticket.");
            }

            _ticketRepository.DeleteTicket(ticket);
            await _ticketRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteTicketImageAsync(int ticketId, string userId, bool isAdmin)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket not found.");
            }

            if (!isAdmin && ticket.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Not allowed to delete this ticket image.");
            }

            if (string.IsNullOrEmpty(ticket.ImageUrl))
            {
                return false; 
            }

            try
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ticket.ImageUrl.TrimStart('/'));

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }

                ticket.ImageUrl = null;
                _ticketRepository.UpdateTicket(ticket);
                await _ticketRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                return false;
            }
        }


    }
}
