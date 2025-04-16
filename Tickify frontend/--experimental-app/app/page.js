"use client";
import "./styles/HomePage.css";
import { useAuth } from "./context/AuthContext";
import Link from "next/link";

export default function HomePage() {
  const { user } = useAuth();

  const isRegularUser =
    user && !user.roles?.includes("Admin") && !user.roles?.includes("SuperAdmin");

  return (
    <div className="home-container">
      <div className="glass-card">
        <h1>Welcome to Tickify!</h1>

        <p>
          Have a question, issue, or request? Just open a ticket and our team will take it from there.
        </p>

        <p>
          Whether it’s a bug, feedback, a request for support, or anything else — we’ve got your back.
        </p>

        <p>
          We’ll make sure to keep you informed as things move forward.
        </p>

        {isRegularUser && (
          <Link href="/tickets/create">
            <button className="cta-button">Create New Ticket</button>
          </Link>
        )}
      </div>
    </div>
  );
}
