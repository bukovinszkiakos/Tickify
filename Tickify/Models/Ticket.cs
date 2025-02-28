using Microsoft.AspNetCore.Identity;

namespace Tickify.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Open"; // például: Open, In Progress, Resolved, Closed
        public string Priority { get; set; } = "Normal";

        // Külső kulcsok
        public string CreatedBy { get; set; }
        public IdentityUser Creator { get; set; }

        public string? AssignedTo { get; set; } // opcionális
        public IdentityUser Assignee { get; set; }

        // Navigációs tulajdonságok
        public ICollection<TicketComment> Comments { get; set; }
        public ICollection<TicketHistory> Histories { get; set; }
        public TicketReview Review { get; set; }
    }

}
