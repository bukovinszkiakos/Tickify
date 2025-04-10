"use client";

import React from "react";
import Link from "next/link";
import { useAuth } from "../../context/AuthContext";
import { Player } from "@lottiefiles/react-lottie-player";
import "../../styles/Navbar.css";
import NotificationBell from "../NotificationBell/NotificationBell"; 

export default function Navbar() {
  const { user, logout } = useAuth();

  async function handleLogout() {
    try {
      await logout();
    } catch (err) {
      console.error("Logout error:", err);
    }
  }

  const isAdmin = user?.roles?.includes("Admin");
  const isSuperAdmin = user?.roles?.includes("SuperAdmin");

  return (
    <header className="heading">
      <nav className="navbar">
        <div className="navbar-links">
          <Link href="/" className="nav-link">
            <button className="nav-button">
              Home
              <Player
                src="/animations/home-animation.json"
                className="animation"
                autoplay
                loop
              />
            </button>
          </Link>

          {user ? (
            <>
             
              {!isAdmin && !isSuperAdmin && (
                <Link href="/tickets" className="nav-link">
                  <button className="nav-button">
                    My Tickets
                    <Player
                      src="/animations/tickets-animation.json"
                      className="animation"
                      autoplay
                      loop
                    />
                  </button>
                </Link>
              )}

              
              {(isAdmin || isSuperAdmin) && (
                <>
                  <Link href="/admin/tickets" className="nav-link">
                    <button className="nav-button">
                      Manage
                      <Player
                        src="/animations/manage-animation.json"
                        className="animation"
                        autoplay
                        loop
                      />
                    </button>
                  </Link>

                  <Link href="/admin" className="nav-link">
                    <button className="nav-button">
                      Admin
                      <Player
                        src="/animations/admin-animation.json"
                        className="animation"
                        autoplay
                        loop
                      />
                    </button>
                  </Link>
                </>
              )}

              
              <NotificationBell />
            </>
          ) : (
            <>
              <Link href="/login" className="nav-link">
                <button className="nav-button">
                  Login
                  <Player
                    src="/animations/login-animation.json"
                    className="animation"
                    autoplay
                    loop
                  />
                </button>
              </Link>

              <Link href="/register" className="nav-link">
                <button className="nav-button">
                  Register
                  <Player
                    src="/animations/register-animation.json"
                    className="animation"
                    autoplay
                    loop
                  />
                </button>
              </Link>
            </>
          )}
        </div>

        {user && (
          <button className="logout-button" onClick={handleLogout}>
            Logout
            <Player
              src="/animations/logout-animation.json"
              className="animation"
              autoplay
              loop
            />
          </button>
        )}
      </nav>
    </header>
  );
}
