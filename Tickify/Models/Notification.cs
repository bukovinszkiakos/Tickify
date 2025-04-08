public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }         // Ki kapja az értesítést
    public string Message { get; set; }        // Pl. "Admin replied to your ticket"
    public string? TicketId { get; set; }      // Hova mutat az értesítés
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}
