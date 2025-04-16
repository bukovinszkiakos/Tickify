"use client";

import React, { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { apiGet } from "../../utils/api";
import Link from "next/link";
import { useRouter } from "next/navigation";
import "../styles/TicketsPage.css";

export default function TicketsPage() {
  const { user } = useAuth();
  const router = useRouter();
  const [tickets, setTickets] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    if (user === null) return;

    if (user?.isAdmin || user?.isSuperAdmin) {
      router.push("/admin/tickets");
      return;
    }

    apiGet("/api/tickets")
      .then((data) => setTickets(data))
      .catch((err) => setError(err.message));
  }, [user, router]);

  if (!user) {
    return <p className="error-message">Please login to see your tickets.</p>;
  }

  if (error) {
    return <p className="error-message">Error: {error}</p>;
  }

  return (
    <section className="tickets-container" aria-labelledby="my-tickets-heading">
      <br />
      <h1 id="my-tickets-heading">🎟️ My Tickets</h1>

      <ul className="ticket-list-condensed">
        {tickets.map((ticket) => (
          <li key={ticket.id} className="ticket-row">
            <div className="ticket-left">
              <Link
                href={`/tickets/${ticket.id}`}
                className="ticket-title-link"
                title={ticket.title}
              >
                {ticket.title}
              </Link>
            </div>

            <div className="ticket-right-info">
              <span
                className={`status-pill status-${ticket.status
                  .toLowerCase()
                  .replace(" ", "-")}`}
              >
                {getStatusIcon(ticket.status)} {ticket.status}
              </span>

              <span className="comment-stats">
                💬 {ticket.totalCommentCount} comment
                {ticket.totalCommentCount !== 1 && "s"}
                {ticket.unreadCommentCount > 0 && (
                  <span className="unread-badge">
                    {" "}
                    ({ticket.unreadCommentCount} new)
                  </span>
                )}
              </span>
            </div>
          </li>
        ))}
      </ul>

      <Link
        href="/tickets/create"
        className="create-ticket-button"
      >
        + Create New Ticket
      </Link>
    </section>
  );
}

function getStatusIcon(status) {
  switch (status.toLowerCase()) {
    case "open":
      return "🟦";
    case "in progress":
      return "🟠";
    case "resolved":
      return "✅";
    case "closed":
      return "❌";
    default:
      return "📄";
  }
}
