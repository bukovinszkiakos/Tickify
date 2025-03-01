"use client";
import React, { createContext, useContext, useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { apiGet, apiPost } from "../../utils/api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const router = useRouter();

  async function fetchUser() {
    try {
      const res = await apiGet("/Auth/Me");
      console.log("Fetched user data:", res);
  
      if (res.roles && Array.isArray(res.roles)) {
        res.isAdmin = res.roles.some(role => role.toLowerCase() === "admin");
      } else {
        res.isAdmin = false;  
      }
  
      setUser(res);
    } catch (err) {
      console.error("Error fetching user:", err);
      setUser(null);
    }
  }
  

  useEffect(() => {
    fetchUser();
  }, []);

  async function login(email, password) {
    try {
      await apiPost("/Auth/Login", { Email: email, Password: password });
      const userData = await fetchUser(); 
      if (userData?.isAdmin) {
        router.push("/admin");
      } else {
        router.push("/tickets");
      }
    } catch (err) {
      console.error("Login error:", err);
    }
  }

  async function logout() {
    try {
      await apiPost("/Auth/Logout", {});
      setUser(null);
      router.push("/login");
    } catch (err) {
      console.error("Logout error:", err);
    }
  }

  return (
    <AuthContext.Provider value={{ user, login, logout, fetchUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
