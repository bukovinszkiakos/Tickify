"use client";

import React, { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import "../../styles/LoginPage.css";

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const script = document.createElement("script");
    script.type = "module";
    script.src = "https://unpkg.com/ionicons@7.1.0/dist/ionicons/ionicons.esm.js";
    document.body.appendChild(script);
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    try {
      await login(email, password);
    } catch (err) {
      setError("Login failed. Check your credentials.");
    }
  }

  return (
    <div className="login-container">
      <div className="login-box">
        <h2>Login</h2>
        {error && <p className="error-message">{error}</p>}

        <form onSubmit={handleSubmit}>
          <div className="input-box">
            <span className="icon"><ion-icon name="mail"></ion-icon></span>
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder=" "
            />
            <label>Email</label>
          </div>

          <div className="input-box">
            <span className="icon"><ion-icon name="lock-closed"></ion-icon></span>
            <input
              type="password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder=" "
            />
            <label>Password</label>
          </div>

          <button type="submit" className="btn">Login</button>

          <div className="login-register">
            <p>
              Don’t have an account? <a href="/register">Register</a>
            </p>
          </div>
        </form>
      </div>
    </div>
  );
}
