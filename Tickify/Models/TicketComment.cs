using Microsoft.AspNetCore.Identity;

namespace Tickify.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public int CommentedBy { get; set; }
        public IdentityUser Commenter { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
