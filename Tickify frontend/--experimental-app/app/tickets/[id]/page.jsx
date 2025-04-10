"use client";
import React, { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from "next/navigation";
import { useAuth } from "../../context/AuthContext";
import "../../styles/TicketDetailPage.css";

export default function TicketDetailPage() {
  const router = useRouter();
  const { id } = useParams();
  const { user } = useAuth();

  const [ticket, setTicket] = useState(null);
  const [originalImageUrl, setOriginalImageUrl] = useState(null);
  const [error, setError] = useState("");
  const [editMode, setEditMode] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState("");
  const [status, setStatus] = useState("");
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState("");
  const [newImage, setNewImage] = useState(null);
  const [editImage, setEditImage] = useState(null);
  const [previewImage, setPreviewImage] = useState(null);
  const [previewChanges, setPreviewChanges] = useState(null);
  const [oldImageUrl, setOldImageUrl] = useState(null);
  const [newImageUrl, setNewImageUrl] = useState(null);
  const [textChanges, setTextChanges] = useState([]);

  const commentsTopRef = useRef(null);

  const fetchTicketData = async () => {
    const res = await fetch(`/api/tickets/${id}`, { credentials: "include" });
    const data = await res.json();
    setTicket(data);
    setTitle(data.title);
    setDescription(data.description);
    setPriority(data.priority);
    setStatus(data.status);
    setOriginalImageUrl(data.imageUrl);
  };

  const fetchComments = async () => {
    const res = await fetch(`/api/tickets/${id}/comments`, {
      credentials: "include",
    });
    const data = await res.json();
    setComments(data);
  };

  useEffect(() => {
    if (!id) return;

    fetchTicketData();
    fetchComments();

    if (user?.isAdmin) {
      fetch(`/api/admin/tickets/${id}/mark-comments-read`, {
        method: "POST",
        credentials: "include"
      });
    }

  }, [id, user]);

  const handleUpdate = async (e) => {
    e.preventDefault();
    const formData = new FormData();
    formData.append("title", title);
    formData.append("description", description);
    formData.append("priority", priority);
    formData.append("status", status);
    if (editImage) {
      formData.append("image", editImage);
    }

    try {
      const res = await fetch(`/api/tickets/${id}`, {
        method: "PUT",
        body: formData,
        credentials: "include",
      });

      if (!res.ok) throw new Error("Failed to update ticket");

      setEditMode(false);
      await fetchTicketData();
      await fetchComments();
    } catch (err) {
      setError(err.message);
    }
  };

  const handleDelete = async () => {
    try {
      await fetch(`/api/tickets/${id}`, {
        method: "DELETE",
        credentials: "include",
      });
      router.push("/tickets");
    } catch (err) {
      setError(err.message);
    }
  };

  const handleAddComment = async (e) => {
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

      if (!response.ok) throw new Error("Failed to add comment");

      setNewComment("");
      setNewImage(null);
      await fetchComments();

      setTimeout(() => {
        commentsTopRef.current?.scrollIntoView({ behavior: "smooth" });
      }, 100);
    } catch (err) {
      setError("Failed to add comment. Please try again.");
    }
  };

  const handleSeeChanges = (commentText) => {
    setPreviewChanges(commentText);

    const lines = commentText
      .split("\n")
      .filter(line =>
        line.startsWith("Title:") ||
        line.startsWith("Description:") ||
        line.startsWith("Priority:") ||
        line.startsWith("Assigned To:")
      );
    setTextChanges(lines);

    const oldMatch = commentText.match(/Old image: (https?:\/\/\S+)/);
    const newMatch = commentText.match(/New image: (https?:\/\/\S+)/);

    if (commentText.includes("🖼️ Image updated.") && oldMatch && newMatch) {
      setOldImageUrl(oldMatch[1]);
      setNewImageUrl(newMatch[1]);
    } else {
      setOldImageUrl(null);
      setNewImageUrl(null);
    }
  };

  if (error) return <p className="error-message">{error}</p>;
  if (!ticket) return <p className="loading-message">Loading ticket...</p>;

  return (
    <div className="ticket-page-container">
      <div className="ticket-detail-card">
        <h1>Ticket Detail</h1>
        <div className="ticket-info">
          <div className="info-block">
            <strong>Title:</strong>
            {editMode ? (
              <input
                className="field-input"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            ) : (
              <div className="scrollable-text">{ticket.title}</div>
            )}
          </div>

          <div className="info-block">
            <strong>Description:</strong>
            {editMode ? (
              <textarea
                className="field-input"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            ) : (
              <div className="scrollable-text">{ticket.description}</div>
            )}
          </div>

          <p><strong>Priority:</strong> {ticket.priority}</p>
          <p><strong>Status:</strong> {ticket.status}</p>

          {editMode && (
            <div className="info-block">
              <strong>Change Screenshot:</strong>
              <input
                type="file"
                accept="image/*"
                onChange={(e) => setEditImage(e.target.files[0])}
              />
            </div>
          )}
        </div>

        <div className="action-buttons">
          {!user?.isAdmin && (
            <button className="edit-btn" onClick={() => setEditMode(!editMode)}>
              {editMode ? "Cancel Edit" : "Edit Ticket"}
            </button>
          )}
          <button className="delete-btn" onClick={handleDelete}>
            Delete Ticket
          </button>
        </div>

        {editMode && !user?.isAdmin && (
          <form className="edit-form" onSubmit={handleUpdate}>
            <div className="action-buttons">
              <button type="submit" className="save-btn">
                Save Changes
              </button>
            </div>
          </form>
        )}

        {ticket.imageUrl && !editMode && (
          <div className="screenshot-section">
            <button onClick={() => setPreviewImage(ticket.imageUrl)}>
              Screenshot or attachment
            </button>
          </div>
        )}
      </div>

      <div className="ticket-comments-section">
        <h2>Comments</h2>
        <div className="comment-scroll">
          <ul className="comment-list">
            <div ref={commentsTopRef}></div>
            {[...comments]
              .reverse()
              .filter(c => !c.comment.startsWith("Ticket created with image:"))
              .map((c) => (
                <li key={c.id} className="comment-item">
                  <p>
                    <strong>{c.commenter || "Unknown"}</strong>{" "}
                    <em>({new Date(c.createdAt).toLocaleString()})</em>
                  </p>
                  {c.comment.startsWith("🔄 Ticket updated:") ? (
                    <>
                      <p className="change-preview">This ticket has been updated.</p>
                      <button className="see-changes-btn" onClick={() => handleSeeChanges(c.comment)}>
                        See Changes
                      </button>
                    </>
                  ) : (
                    <p>{c.comment}</p>
                  )}
                  {c.imageUrl && (
                    <button onClick={() => setPreviewImage(c.imageUrl)}>Screenshot</button>
                  )}
                </li>
              ))}
          </ul>
        </div>

        <form onSubmit={handleAddComment} className="comment-form" encType="multipart/form-data">
          <textarea
            placeholder="Write a comment..."
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            required
          />
          <input
            type="file"
            accept="image/*"
            onChange={(e) => setNewImage(e.target.files[0])}
          />
          <button type="submit">Submit Comment</button>
        </form>
      </div>

      {previewImage && (
        <div className="preview-modal image-overlay" onClick={() => setPreviewImage(null)}>
          <img src={previewImage} alt="Preview" className="preview-full" />
        </div>
      )}

      {previewChanges && (
        <div className="preview-modal" onClick={() => setPreviewChanges(null)}>
          <div className="preview-content" onClick={(e) => e.stopPropagation()}>
            <h3>📋 Ticket Changes</h3>
            <p className="change-preview">📝 Changes made to the ticket.</p>

            {textChanges.length > 0 && <pre>{textChanges.join("\n")}</pre>}

            {oldImageUrl && (
              <button className="image-button" onClick={() => setPreviewImage(oldImageUrl)}>
                Old Image
              </button>
            )}
            {newImageUrl && (
              <button className="image-button" onClick={() => setPreviewImage(newImageUrl)}>
                New Image
              </button>
            )}

            <button className="close-btn" onClick={() => setPreviewChanges(null)}>
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
