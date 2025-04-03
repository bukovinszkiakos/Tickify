using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Tickify.Services;
using Tickify.DTOs;
using Microsoft.Extensions.Hosting;

namespace Tickify.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITicketService _ticketService;

        public AdminController(UserManager<IdentityUser> userManager, ITicketService ticketService)
        {
            _userManager = userManager;
            _ticketService = ticketService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            return Ok(users);
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            await _userManager.DeleteAsync(user);
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost("users/{userId}/role/{role}")]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = $"User {user.UserName} assigned to role {role}" });
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] string status = "", [FromQuery] string priority = "")
        {
            var tickets = await _ticketService.GetTicketsForUserAsync(null, true);

            if (!string.IsNullOrEmpty(status))
                tickets = tickets.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                tickets = tickets.Where(t => t.Priority == priority);

            return Ok(tickets);
        }

        [HttpPut("tickets/{id}/status/{newStatus}")]
        public async Task<IActionResult> UpdateTicketStatus(int id, string newStatus)
        {
            var allowedStatuses = new[] { "Open", "In Progress", "Resolved", "Closed" };

            if (!allowedStatuses.Contains(newStatus))
            {
                return BadRequest($"Invalid status. Allowed statuses: {string.Join(", ", allowedStatuses)}");
            }

            try
            {
                var adminName = User.Identity?.Name ?? "Admin";
                await _ticketService.UpdateTicketStatusAsync(id, newStatus, adminName);
                return Ok(new { message = "Ticket status updated" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }



        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var tickets = await _ticketService.GetTicketsForUserAsync(null, true);

            var openTickets = tickets.Count(t => t.Status == "Open");
            var inProgressTickets = tickets.Count(t => t.Status == "In Progress");
            var resolvedTickets = tickets.Count(t => t.Status == "Resolved");
            var closedTickets = tickets.Count(t => t.Status == "Closed");

            return Ok(new
            {
                openTickets,
                inProgressTickets,
                resolvedTickets,
                closedTickets,
                totalTickets = tickets.Count()
            });
        }
    }
}
