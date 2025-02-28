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

        public async Task<IEnumerable<TicketDto>> GetAllTicketsDtoAsync()
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
            return ticketDtos;
        }

        public async Task<TicketDto> GetTicketDtoByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null) return null;
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
                AssignedTo = ticket.AssignedTo
            };
        }

        public async Task<TicketDto> CreateTicketAsync(CreateTicketDto createDto, string userId)
        {
            var ticket = new Ticket
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Priority = createDto.Priority,
                Status = "Open",
                CreatedBy = userId, 
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
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
                AssignedTo = ticket.AssignedTo
            };
        }

        public async Task UpdateTicketAsync(int id, UpdateTicketDto updateDto)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                throw new Exception("Ticket not found");
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

        public async Task DeleteTicketAsync(int id)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket != null)
            {
                _ticketRepository.DeleteTicket(ticket);
                await _ticketRepository.SaveChangesAsync();
            }
        }
    }


}
