"use client";
import React from "react";
import Link from "next/link";
import { useAuth } from "../../context/AuthContext";
import "../../styles/Navbar.css"; 

export default function Navbar() {
  const { user, logout } = useAuth();

  async function handleLogout() {
    try {
      await logout();
    } catch (err) {
      console.error("Logout error:", err);
    }
  }

  return (
    <nav className="navbar">
      <div className="navbar-links">
        <Link href="/">Home</Link>

        {user ? (
          <>
            {!user.isAdmin && <Link href="/tickets">My Tickets</Link>}
            {user.isAdmin && <Link href="/admin/tickets">Manage Tickets</Link>}
            {user.isAdmin && <Link href="/admin">Admin</Link>}
          </>
        ) : (
          <>
            <Link href="/login">Login</Link>
            <Link href="/register">Register</Link>
          </>
        )}
      </div>

      {user && (
        <button className="logout-button" onClick={handleLogout}>
          Logout
        </button>
      )}
    </nav>
  );
}
