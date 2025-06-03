"use client";

import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { apiPost } from "../../../utils/api";
import Link from "next/link";
import "../../styles/RegisterPage.css";

export default function RegisterPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const script = document.createElement("script");
    script.type = "module";
    script.src = "https://unpkg.com/ionicons@7.1.0/dist/ionicons/ionicons.esm.js";
    document.body.appendChild(script);
  }, []);

  async function handleRegister(e) {
    e.preventDefault();
    setError("");
    try {
      await apiPost("/Auth/Register", {
        Email: email,
        Username: username,
        Password: password,
      });
      router.push("/login");
    } catch (err) {
      setError("Registration failed.");
    }
  }

  return (
    <div className="register-container">
      <div className="register-card-wrapper">
        <div className="info-card fadeIn">
          <div className="info-card-text">
            <h3>Why Join Tickify?</h3>
            <p>
              Get instant updates, manage your tickets efficiently, and collaborate with our team —
              all in one intuitive platform.
            </p>
          </div>
          <img
            src="/images/register-illustration.png"
            alt="Register Illustration"
            className="info-image"
          />
        </div>

        <div className="register-box fadeIn">
          <h2>
            Register <ion-icon name="person-add-outline"></ion-icon>
          </h2>
          {error && <p className="error-message">{error}</p>}

          <form onSubmit={handleRegister}>
            <div className="input-box">
              <span className="icon"><ion-icon name="person"></ion-icon></span>
              <input
                type="text"
                required
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder=" "
              />
              <label>Username</label>
            </div>

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

            <button type="submit" className="btn">Register</button>

            <div className="login-register">
              <p>
                Already have an account? <Link href="/login">Login</Link>
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
