"use client";
import "./styles/HomePage.css";
import { useAuth } from "./context/AuthContext";
import Link from "next/link";

export default function HomePage() {
  const { user } = useAuth();

  return (
    <div className="home-container">
      <div className="glass-card">
        <h1> Welcome to Tickify!</h1>
        <p>
          Something not working? Need help with anything? Just create a ticket and we’ll take it from there.
        </p>
        <p>
          Whether it’s a glitch, a broken tool, or a general request — send it in with a quick description or screenshot.
        </p>
        <p>
          Our team is here to help and will keep you in the loop the whole time.
        </p>

        {user ? (
          <Link href="/tickets/create">
            <button className="cta-button">Create New Ticket</button>
          </Link>
        ) : (
          <p className="login-hint">🔐 Please log in to create a new ticket.</p>
        )}
      </div>
    </div>
  );
}
