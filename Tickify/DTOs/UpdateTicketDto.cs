﻿namespace Tickify.DTOs
{
    public class UpdateTicketDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string? AssignedTo { get; set; }
    }

}
