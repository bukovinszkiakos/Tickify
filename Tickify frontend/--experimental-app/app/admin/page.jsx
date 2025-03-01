"use client";
import React, { useEffect, useState } from "react";
import { apiGet } from "../../utils/api";
import "../styles/AdminDashboardPage.css"; 

export default function AdminDashboardPage() {
  const [stats, setStats] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    apiGet("/api/admin/dashboard")
      .then((data) => setStats(data))
      .catch((err) => setError(err.message));
  }, []);

  if (error) return <p className="error-message">{error}</p>;
  if (!stats) return <p>Loading dashboard...</p>;

  return (
    <div className="admin-dashboard-container">
      <h1>Admin Dashboard</h1>
      <ul className="stats-list">
        <li className="open">Open tickets: {stats.openTickets}</li>
        <li className="in-progress">In Progress: {stats.inProgressTickets}</li>
        <li className="resolved">Resolved: {stats.resolvedTickets}</li>
        <li className="closed">Closed: {stats.closedTickets}</li>
        <li>Total: {stats.totalTickets}</li>
      </ul>

      <div className="admin-links">
        <a href="/admin/users">Manage Users</a>
        <a href="/admin/tickets">Manage Tickets</a>
      </div>
    </div>
  );
}
