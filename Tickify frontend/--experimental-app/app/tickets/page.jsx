"use client";
import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext"; 
import { apiGet } from "../../utils/api";
import "../styles/TicketsPage.css";  

export default function TicketsPage() {
  const { user } = useAuth();
  const [tickets, setTickets] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!user) return;

    apiGet("/api/tickets")
      .then((data) => setTickets(data))
      .catch((err) => setError(err.message));
  }, [user]);

  if (!user) {
    return <p className="error-message">Please login to see your tickets.</p>;
  }

  if (error) {
    return <p className="error-message">Error: {error}</p>;
  }

  return (
    <div className="tickets-container">
      <h1>My Tickets</h1>
      <ul>
        {tickets.map((ticket) => (
          <li key={ticket.id} className="ticket-item">
            <a href={`/tickets/${ticket.id}`} className="ticket-link">
              {ticket.title}
            </a> - {ticket.status}
          </li>
        ))}
      </ul>
      <a href="/tickets/create" className="create-ticket-link">Create New Ticket</a>
    </div>
  );
}
