using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Tickify.DTOs;
using Tickify.Services;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Tickify.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,User")]
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
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            bool isAdmin = roles.Contains("Admin");

            var ticketDtos = await _ticketService.GetTicketsForUserAsync(userId, isAdmin);
            return Ok(ticketDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            bool isAdmin = roles.Contains("Admin");

            try
            {
                var ticketDto = await _ticketService.GetTicketDtoByIdAsync(id, userId, isAdmin);

                if (!string.IsNullOrEmpty(ticketDto.ImageUrl))
                {
                    ticketDto.ImageUrl = $"{Request.Scheme}://{Request.Host}{ticketDto.ImageUrl}";
                }

                return Ok(ticketDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateTicket(
         [FromForm] string title,
         [FromForm] string description,
         [FromForm] string priority,
         [FromForm] IFormFile? image)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            bool isAdmin = HttpContext.User.IsInRole("Admin");

            try
            {
                var ticketDto = await _ticketService.CreateTicketAsync(title, description, priority, userId, isAdmin, image);
                return CreatedAtAction(nameof(GetTicket), new { id = ticketDto.Id }, ticketDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            bool isAdmin = roles.Contains("Admin");

            try
            {
                await _ticketService.UpdateTicketAsync(id, updateDto, userId, isAdmin);
                return Ok(new { message = "Ticket updated successfully" });

            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            bool isAdmin = roles.Contains("Admin");

            try
            {
                await _ticketService.DeleteTicketAsync(id, userId, isAdmin);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}/image")]
        public async Task<IActionResult> DeleteTicketImage(int id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = HttpContext.User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            bool isAdmin = roles.Contains("Admin");

            try
            {
                var result = await _ticketService.DeleteTicketImageAsync(id, userId, isAdmin);
                if (!result)
                {
                    return NotFound("Image not found or already deleted.");
                }
                return Ok(new { message = "Image deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to delete image.");
            }
        }


    }
}
