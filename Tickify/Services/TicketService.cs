using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tickify.Context;
using Tickify.DTOs;
using Tickify.Models;
using Tickify.Repositories;

namespace Tickify.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITicketCommentService _ticketCommentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketService(
     ITicketRepository ticketRepository,
     UserManager<IdentityUser> userManager,
     ITicketCommentService ticketCommentService,
     ApplicationDbContext dbContext,
     IHttpContextAccessor httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
            _userManager = userManager;
            _ticketCommentService = ticketCommentService;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsForUserAsync(string userId, bool isAdmin)
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();

            var commentCounts = await _dbContext.TicketComments
                .GroupBy(c => c.TicketId)
                .Select(g => new
                {
                    TicketId = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            var relevantTicketIds = isAdmin
    ? tickets
        .Where(t =>
            t.AssignedTo == userId ||  
            _dbContext.TicketComments.Any(c => c.TicketId == t.Id && c.CommentedBy == userId) || 
            _dbContext.TicketHistories.Any(h => h.TicketId == t.Id && h.ChangedBy.ToString() == userId)) 
        .Select(t => t.Id)
        .ToList()
    : tickets
        .Where(t => t.CreatedBy == userId)
        .Select(t => t.Id)
        .ToList();






            var userIdString = (userId ?? "").Trim();

            var unreadCounts = await _dbContext.TicketComments
                .Where(c => relevantTicketIds.Contains(c.TicketId))
                .Where(c => c.CommentedBy.Trim() != userIdString) 
                .Where(c => !_dbContext.CommentReadStatuses
                    .Any(r => r.CommentId == c.Id && r.UserId == userIdString))
                .GroupBy(c => c.TicketId)
                .Select(g => new
                {
                    TicketId = g.Key,
                    Unread = g.Count()
                })
                .ToListAsync();



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
                AssignedTo = t.AssignedTo,
                ImageUrl = t.ImageUrl,

                TotalCommentCount = commentCounts.FirstOrDefault(c => c.TicketId == t.Id)?.Total ?? 0,
                UnreadCommentCount = unreadCounts.FirstOrDefault(u => u.TicketId == t.Id)?.Unread ?? 0
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

        public async Task<TicketDto> CreateTicketAsync(
    string title,
    string description,
    string priority,
    string userId,
    bool isAdmin,
    IFormFile? image,
    string scheme,
    string host)
        {
            if (isAdmin)
            {
                throw new UnauthorizedAccessException("Admin users are not allowed to create tickets.");
            }

            string? imageUrl = null;
            string? fullImageUrl = null;

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
                fullImageUrl = $"{scheme}://{host}{imageUrl}";
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

            if (!string.IsNullOrEmpty(fullImageUrl))
            {
                var commentText = $"Ticket created with image: {fullImageUrl}";
                var user = await _userManager.FindByIdAsync(userId);
                await _ticketCommentService.AddCommentAsync(
                    ticket.Id,
                    commentText,
                    userId,
                    user?.UserName ?? "Unknown",
                    null
                );
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
                ImageUrl = imageUrl
            };
        }



        public async Task UpdateTicketAsync(
     int id,
     UpdateTicketDto updateDto,
     string userId,
     bool isAdmin,
     IFormFile? image,
     string scheme,
     string host)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(id);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found.");

            if (!isAdmin && ticket.CreatedBy != userId)
                throw new UnauthorizedAccessException("Not allowed to update this ticket.");

            var changes = new List<string>();
            string? oldImageUrl = null;
            string? newImageUrl = null;
            bool imageChanged = false;

            if (!string.IsNullOrEmpty(ticket.ImageUrl))
            {
                oldImageUrl = $"{scheme}://{host}{ticket.ImageUrl}";
            }

            if (ticket.Title != updateDto.Title)
            {
                changes.Add($"Title: \"{ticket.Title}\" → \"{updateDto.Title}\"");
                ticket.Title = updateDto.Title;
            }

            if (ticket.Description != updateDto.Description)
            {
                changes.Add($"Description: \"{ticket.Description}\" → \"{updateDto.Description}\"");
                ticket.Description = updateDto.Description;
            }

            if (ticket.Priority != updateDto.Priority)
            {
                changes.Add($"Priority: {ticket.Priority} → {updateDto.Priority}");
                ticket.Priority = updateDto.Priority;
            }

            if (ticket.AssignedTo != updateDto.AssignedTo)
            {
                changes.Add($"Assigned To: {ticket.AssignedTo ?? "None"} → {updateDto.AssignedTo ?? "None"}");
                ticket.AssignedTo = updateDto.AssignedTo;
            }

            if (image != null && image.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var newPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                ticket.ImageUrl = $"/uploads/{fileName}";
                newImageUrl = $"{scheme}://{host}{ticket.ImageUrl}";
                imageChanged = true;

                changes.Add("🖼️ Image updated.");
            }

            ticket.UpdatedAt = DateTime.Now;
            _ticketRepository.UpdateTicket(ticket);

            if (changes.Any())
            {
                var commentText = "🔄 Ticket updated:\n" + string.Join("\n", changes);

                if (imageChanged && oldImageUrl != null && newImageUrl != null)
                {
                    commentText += $"\nOld image: {oldImageUrl}\nNew image: {newImageUrl}";
                }

                var user = await _userManager.FindByIdAsync(userId);
                await _ticketCommentService.AddCommentAsync(
                    ticket.Id,
                    commentText,
                    userId,
                    user?.UserName ?? "Unknown",
                    null
                );
            }

            await _ticketRepository.SaveChangesAsync();
        }



        public async Task UpdateTicketStatusAsync(int ticketId, string newStatus, string adminName)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null)
                throw new KeyNotFoundException("Ticket not found.");

            var oldStatus = ticket.Status;
            if (oldStatus == newStatus) return;

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.Now;
            _ticketRepository.UpdateTicket(ticket);

            var adminId = (_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "").Trim();

            var commentText = $"🔁 Status changed by admin ({adminName}): {oldStatus} → {newStatus}";
            await _ticketCommentService.AddCommentAsync(
                ticket.Id,
                commentText,
                adminId,        
                adminName,
                null
            );

            var creatorId = (ticket.CreatedBy ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(creatorId) &&
                !string.IsNullOrWhiteSpace(adminId) &&
                !string.Equals(creatorId, adminId, StringComparison.OrdinalIgnoreCase))
            {
                await _dbContext.Notifications.AddAsync(new Notification
                {
                    UserId = creatorId,
                    CreatedBy = adminId,
                    Message = $"📌 Your ticket \"{ticket.Title}\" status changed to \"{newStatus}\".",
                    TicketId = ticket.Id.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });

                await _dbContext.SaveChangesAsync();
            }
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

        public async Task<IEnumerable<TicketDto>> GetTicketsForAdminAsync(string adminUserId)
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();
            var userIdString = adminUserId.Trim();

            var allTicketIds = tickets.Select(t => t.Id).ToList();

            var commentCounts = await _dbContext.TicketComments
                .GroupBy(c => c.TicketId)
                .Select(g => new {
                    TicketId = g.Key,
                    Total = g.Count()
                }).ToListAsync();

                    var unreadCounts = await _dbContext.TicketComments
             .Where(c => allTicketIds.Contains(c.TicketId))
             .Where(c => c.CommentedBy != userIdString && c.CommentedBy != "admin-system") 
             .Where(c => !_dbContext.CommentReadStatuses
                 .Any(r => r.CommentId == c.Id && r.UserId == userIdString))
             .GroupBy(c => c.TicketId)
             .Select(g => new {
                 TicketId = g.Key,
                 Unread = g.Count()
             }).ToListAsync();


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
                AssignedTo = t.AssignedTo,
                ImageUrl = t.ImageUrl,
                TotalCommentCount = commentCounts.FirstOrDefault(c => c.TicketId == t.Id)?.Total ?? 0,
                UnreadCommentCount = unreadCounts.FirstOrDefault(u => u.TicketId == t.Id)?.Unread ?? 0
            });

            return ticketDtos;
        }


        public async Task MarkTicketCommentsAsReadAsync(int ticketId, string userId)
        {
            var userIdString = userId.Trim();

            var unreadCommentIds = await _dbContext.TicketComments
                .Where(c => c.TicketId == ticketId)
                .Where(c => c.CommentedBy != userIdString)
                .Where(c => !_dbContext.CommentReadStatuses
                    .Any(r => r.CommentId == c.Id && r.UserId == userIdString))
                .Select(c => c.Id)
                .ToListAsync();

            var newStatuses = unreadCommentIds.Select(id => new CommentReadStatus
            {
                CommentId = id,
                UserId = userIdString,
                SeenAt = DateTime.UtcNow
            }).ToList();

            if (newStatuses.Any())
            {
                _dbContext.CommentReadStatuses.AddRange(newStatuses);
                await _dbContext.SaveChangesAsync();
            }
        }


    }
}
