﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Tickify.DTOs;
using Tickify.Services;
using System.IO;

namespace Tickify.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,User")]
    [Route("api/tickets/{ticketId}/comments")]
    public class TicketCommentsController : ControllerBase
    {
        private readonly ITicketCommentService _ticketCommentService;
        private readonly UserManager<IdentityUser> _userManager;

        public TicketCommentsController(ITicketCommentService ticketCommentService, UserManager<IdentityUser> userManager)
        {
            _ticketCommentService = ticketCommentService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int ticketId)
        {
            var comments = await _ticketCommentService.GetCommentsByTicketIdAsync(ticketId);

            var result = comments.Select(c => new {
                c.Id,
                c.Comment,
                c.CreatedAt,
                c.ImageUrl, 
                Commenter = c.Commenter != null ? c.Commenter.UserName : "Unknown"
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int ticketId, [FromForm] string comment, [FromForm] IFormFile? image)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not found in token.");

            string imageUrl = null;

            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{image.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}"; 
            }

            try
            {
                await _ticketCommentService.AddCommentAsync(ticketId, comment, userId, imageUrl);
                return Ok(new { message = "Comment added successfully", imageUrl });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
