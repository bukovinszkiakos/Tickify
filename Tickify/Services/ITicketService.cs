using Tickify.DTOs;
using Tickify.Models;

namespace Tickify.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDto>> GetAllTicketsDtoAsync();
        Task<TicketDto> GetTicketDtoByIdAsync(int id);
        Task<TicketDto> CreateTicketAsync(CreateTicketDto createDto, string userId);
        Task UpdateTicketAsync(int id, UpdateTicketDto updateDto);
        Task DeleteTicketAsync(int id);
    }


}
