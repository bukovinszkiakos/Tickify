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
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    Roles = roles 
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("SuperAdmin") && User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid("Admins cannot delete SuperAdmins.");
            }

            await _userManager.DeleteAsync(user);
            return Ok(new { message = "User deleted successfully" });
        }


        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("users/{userId}/role/{role}")]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded) return BadRequest(addResult.Errors);

            return Ok(new { message = $"User {user.UserName} role set to {role}" });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] string status = "", [FromQuery] string priority = "")
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var tickets = await _ticketService.GetTicketsForAdminAsync(userId); 

            if (!string.IsNullOrEmpty(status))
                tickets = tickets.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                tickets = tickets.Where(t => t.Priority == priority);

            return Ok(tickets);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
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


        [Authorize(Roles = "Admin,SuperAdmin")]
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

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("tickets/{ticketId}/mark-comments-read")]
        public async Task<IActionResult> MarkCommentsAsRead(int ticketId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _ticketService.MarkTicketCommentsAsReadAsync(ticketId, userId);
            return Ok(new { message = "Comments marked as read." });
        }

    }
}
