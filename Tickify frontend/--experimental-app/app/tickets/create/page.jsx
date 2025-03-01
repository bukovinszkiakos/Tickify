"use client";
import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "../../context/AuthContext";

export default function CreateTicketPage() {
  const { user } = useAuth();
  const router = useRouter();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState("Normal");
  const [error, setError] = useState("");
  const [image, setImage] = useState(null);

  useEffect(() => {
    if (user === null) return;
    if (!user) {
      router.push("/login");
    } else if (user.isAdmin) {
      router.push("/tickets");
    }
  }, [user, router]);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
  
    const formData = new FormData();
    formData.append("title", title);
    formData.append("description", description);
    formData.append("priority", priority);
    if (image) {
      formData.append("image", image);
    }
  
    try {
      const response = await fetch("/api/tickets", {
        method: "POST",
        body: formData,
        credentials: "include",
      });
  
      if (!response.ok) {
        throw new Error("Failed to create ticket");
      }
  
      router.push("/tickets");
    } catch (err) {
      setError(err.message);
    }
  }
  

  return (
    <div className="create-ticket-container">
      <h1>Create Ticket</h1>
      {error && <p className="error-message">{error}</p>}
      <form onSubmit={handleSubmit} encType="multipart/form-data">
        <input
          type="text"
          placeholder="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
        <br />
        <textarea
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
        />
        <br />
        <select value={priority} onChange={(e) => setPriority(e.target.value)}>
          <option>Low</option>
          <option>Normal</option>
          <option>High</option>
        </select>
        <br />
        <input
          type="file"
          accept="image/*"
          onChange={(e) => setImage(e.target.files[0])}
        />
        <br />
        <button type="submit">Create</button>
      </form>
    </div>
  );
}
