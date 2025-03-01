"use client";
import React, { useEffect, useState } from "react";
import Link from "next/link";
import { apiGet, apiPut } from "../../../utils/api";
import "../../styles/AdminTicketsPage.css"; 

export default function AdminTicketsPage() {
  const [tickets, setTickets] = useState([]);
  const [error, setError] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterPriority, setFilterPriority] = useState("");

  useEffect(() => {
    fetchTickets();
  }, []);

  async function fetchTickets() {
    try {
      const data = await apiGet("/api/admin/tickets");
      setTickets(data);
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleUpdateStatus(ticketId, newStatus) {
    try {
      await apiPut(`/api/admin/tickets/${ticketId}/status/${newStatus}`, {});
      setTickets((prev) =>
        prev.map((t) =>
          t.id === ticketId ? { ...t, status: newStatus } : t
        )
      );
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div className="admin-tickets-container">
      <h1>Manage Tickets</h1>

      <div className="filters">
        <label>
          Status Filter:
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
          >
            <option value="">All</option>
            <option>Open</option>
            <option>In Progress</option>
            <option>Resolved</option>
            <option>Closed</option>
          </select>
        </label>

        <label>
          Priority Filter:
          <select
            value={filterPriority}
            onChange={(e) => setFilterPriority(e.target.value)}
          >
            <option value="">All</option>
            <option>Low</option>
            <option>Normal</option>
            <option>High</option>
          </select>
        </label>

        <button onClick={fetchTickets}>Apply Filters</button>
      </div>

      {error && <p className="error-message">{error}</p>}

      <ul className="ticket-list">
        {tickets
          .filter((t) => !filterStatus || t.status === filterStatus)
          .filter((t) => !filterPriority || t.priority === filterPriority)
          .map((ticket) => (
            <li key={ticket.id} className="ticket-item">
              <Link href={`/tickets/${ticket.id}`}>
                <strong>{ticket.title}</strong>
              </Link>{" "}
              - {ticket.status} / {ticket.priority}
              <div className="status-buttons">
                <button
                  className="status-open"
                  onClick={() => handleUpdateStatus(ticket.id, "Open")}
                >
                  Open
                </button>
                <button
                  className="status-inprogress"
                  onClick={() => handleUpdateStatus(ticket.id, "In Progress")}
                >
                  In Progress
                </button>
                <button
                  className="status-resolved"
                  onClick={() => handleUpdateStatus(ticket.id, "Resolved")}
                >
                  Resolved
                </button>
                <button
                  className="status-closed"
                  onClick={() => handleUpdateStatus(ticket.id, "Closed")}
                >
                  Closed
                </button>
              </div>
            </li>
          ))}
      </ul>
    </div>
  );
}
