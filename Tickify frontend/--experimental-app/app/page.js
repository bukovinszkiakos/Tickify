"use client";
import "./styles/HomePage.css";

export default function HomePage() {
  return (
    <div className="home-container">
      <div className="instructions-box">
        <h1>Welcome to Tickify!</h1>
        <p>
          Something not working? Need help with anything? Just create a ticket and we’ll take it from there.
        </p>
        <p>
          Whether it's a technical glitch, a broken tool, or just a general request — submit your ticket with a quick description or an image.
        </p>
        <p>
          Our team will review it and keep you updated every step of the way.
        </p>
      </div>
    </div>
  );
}
