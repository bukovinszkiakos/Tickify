"use client";
import React, { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { apiGet, apiPut, apiDelete, apiPost } from "../../../utils/api";

export default function TicketDetailPage() {
  const router = useRouter();
  const { id } = useParams();
  const [ticket, setTicket] = useState(null);
  const [error, setError] = useState("");
  const [editMode, setEditMode] = useState(false);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState("");
  const [status, setStatus] = useState("");

  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState("");
  const [newImage, setNewImage] = useState(null);
  const [previewImage, setPreviewImage] = useState(null); 

  useEffect(() => {
    if (!id) return;
    apiGet(`/api/tickets/${id}`)
      .then((data) => {
        setTicket(data);
        setTitle(data.title);
        setDescription(data.description);
        setPriority(data.priority);
        setStatus(data.status);
      })
      .catch((err) => setError(err.message));

    apiGet(`/api/tickets/${id}/comments`)
      .then((data) => setComments(data))
      .catch((err) => console.error("Error fetching comments:", err.message));
  }, [id]);

  async function handleUpdate(e) {
    e.preventDefault();
    try {
      await apiPut(`/api/tickets/${id}`, {
        title,
        description,
        status,
        priority,
      });
      router.push("/tickets");
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleDelete() {
    try {
      await apiDelete(`/api/tickets/${id}`);
      router.push("/tickets");
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleDeleteImage() {
    if (!window.confirm("Are you sure you want to delete this image?")) return;

    try {
      const response = await fetch(`/api/tickets/${id}/image`, {
        method: "DELETE",
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Failed to delete image.");
      }

      setTicket((prev) => ({ ...prev, imageUrl: null })); 
    } catch (err) {
      console.error("Error deleting image:", err.message);
      setError("Failed to delete image.");
    }
  }

  async function handleAddComment(e) {
    e.preventDefault();

    const formData = new FormData();
    formData.append("comment", newComment);
    if (newImage) {
      formData.append("image", newImage);
    }

    try {
      const response = await fetch(`/api/tickets/${id}/comments`, {
        method: "POST",
        body: formData,
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Failed to add comment");
      }

      setNewComment("");
      setNewImage(null);

      const updatedComments = await apiGet(`/api/tickets/${id}/comments`);
      setComments(updatedComments);
    } catch (err) {
      console.error("Error adding comment:", err.message);
      setError("Failed to add comment. Please try again.");
    }
  }

  if (error) {
    return <p style={{ color: "red" }}>{error}</p>;
  }
  if (!ticket) {
    return <p>Loading ticket...</p>;
  }

  return (
    <div style={{ padding: "1rem" }}>
      <h1>Ticket Detail (ID: {id})</h1>

      {ticket.imageUrl && (
        <div style={{ position: "relative", textAlign: "center" }}>
          <img
            src={ticket.imageUrl}
            alt="Ticket Attachment"
            style={{ maxWidth: "200px", cursor: "pointer" }}
            onClick={() => setPreviewImage(ticket.imageUrl)}
          />
          <button
            onClick={handleDeleteImage}
            style={{
              position: "absolute",
              top: "-10px",
              right: "-10px",
              background: "red",
              color: "white",
              border: "none",
              borderRadius: "50%",
              width: "25px",
              height: "25px",
              cursor: "pointer",
            }}
          >
            ❌
          </button>
        </div>
      )}

      {previewImage && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            width: "100vw",
            height: "100vh",
            background: "rgba(0,0,0,0.8)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1000,
          }}
          onClick={() => setPreviewImage(null)}
        >
          <img
            src={previewImage}
            alt="Full size preview"
            style={{ maxWidth: "90%", maxHeight: "90%", borderRadius: "10px" }}
          />
        </div>
      )}

      <p>
        <strong>Title:</strong> {ticket.title}
      </p>
      <p>
        <strong>Description:</strong> {ticket.description}
      </p>
      <p>
        <strong>Priority:</strong> {ticket.priority}
      </p>
      <p>
        <strong>Status:</strong> {ticket.status}
      </p>

      <button onClick={() => setEditMode(!editMode)}>
        {editMode ? "Cancel Edit" : "Edit Ticket"}
      </button>
      <button
        onClick={handleDelete}
        style={{ marginLeft: "1rem", color: "red" }}
      >
        Delete Ticket
      </button>
      {editMode ? (
        <form onSubmit={handleUpdate}>
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
          <select
            value={priority}
            onChange={(e) => setPriority(e.target.value)}
          >
            <option>Low</option>
            <option>Normal</option>
            <option>High</option>
          </select>
          <br />
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            <option>Open</option>
            <option>In Progress</option>
            <option>Resolved</option>
            <option>Closed</option>
          </select>
          <br />
          <button type="submit">Save</button>
        </form>
      ) : null}

      <hr />
      <h2>Comments</h2>
      {comments.length === 0 ? (
        <p>No comments yet.</p>
      ) : (
        <ul>
          {comments.map((c) => (
            <li key={c.id}>
              <strong>{c.commenter}</strong> (
              {new Date(c.createdAt).toLocaleString()}):
              <br />
              {c.comment}
              {c.imageUrl && (
                <>
                  <br />
                  <img
                    src={c.imageUrl}
                    alt="Comment attachment"
                    style={{ width: "100px", cursor: "pointer" }}
                    onClick={() => setPreviewImage(c.imageUrl)}
                  />
                </>
              )}
            </li>
          ))}
        </ul>
      )}
      <form onSubmit={handleAddComment} encType="multipart/form-data">
        <textarea
          placeholder="Add your comment..."
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          required
        />
        <br />
        <input
          type="file"
          accept="image/*"
          onChange={(e) => setNewImage(e.target.files[0])}
        />
        <br />
        <button type="submit">Submit Comment</button>
      </form>
    </div>
  );
}
