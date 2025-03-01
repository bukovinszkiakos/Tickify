"use client";
import React, { useEffect, useState } from "react";
import { apiGet, apiDelete, apiPost } from "../../../utils/api";
import "../../styles/AdminUsersPage.css"; 

export default function AdminUsersPage() {
  const [users, setUsers] = useState([]);
  const [error, setError] = useState("");

  const [roleToAssign, setRoleToAssign] = useState("Admin");
  const [userIdForRole, setUserIdForRole] = useState("");

  useEffect(() => {
    apiGet("/api/admin/users")
      .then((data) => setUsers(data))
      .catch((err) => setError(err.message));
  }, []);

  async function handleDeleteUser(userId) {
    try {
      await apiDelete(`/api/admin/users/${userId}`);
      setUsers((prev) => prev.filter((u) => u.id !== userId));
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleAssignRole(e) {
    e.preventDefault();
    try {
      await apiPost(`/api/admin/users/${userIdForRole}/role/${roleToAssign}`, {});
      alert("Role assigned!");
    } catch (err) {
      setError(err.message);
    }
  }

  if (error) return <p className="error-message">{error}</p>;

  return (
    <div className="admin-users-container">
      <h1>Manage Users</h1>
      <ul>
        {users.map((user) => (
          <li key={user.id}>
            {user.userName} - {user.email}
            <button onClick={() => handleDeleteUser(user.id)}>Delete</button>
          </li>
        ))}
      </ul>

      <hr />
      <h2>Assign Role</h2>
      <form className="assign-role-form" onSubmit={handleAssignRole}>
        <input
          type="text"
          placeholder="User ID"
          value={userIdForRole}
          onChange={(e) => setUserIdForRole(e.target.value)}
          required
        />
        <select
          value={roleToAssign}
          onChange={(e) => setRoleToAssign(e.target.value)}
        >
          <option value="Admin">Admin</option>
          <option value="User">User</option>
        </select>
        <button type="submit">Assign</button>
      </form>
    </div>
  );
}
