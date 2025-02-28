namespace Tickify.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Tickify.DTOs;
    using Microsoft.AspNetCore.Authorization;
    using System.Security.Claims;
    using Tickify.Services;
    using Microsoft.AspNetCore.Identity;

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<IdentityUser> _userManager; 

        public TicketsController(ITicketService ticketService, UserManager<IdentityUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager; 
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var ticketDtos = await _ticketService.GetAllTicketsDtoAsync();
            return Ok(ticketDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticketDto = await _ticketService.GetTicketDtoByIdAsync(id);
            if (ticketDto == null)
                return NotFound();
            return Ok(ticketDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            string userId = userIdClaim.Value;

            Console.WriteLine($"Token userId: {userId}"); 

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found in the database.");

            var ticketDto = await _ticketService.CreateTicketAsync(createDto, userId);
            return CreatedAtAction(nameof(GetTicket), new { id = ticketDto.Id }, ticketDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _ticketService.UpdateTicketAsync(id, updateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            await _ticketService.DeleteTicketAsync(id);
            return NoContent();
        }
    }



}
