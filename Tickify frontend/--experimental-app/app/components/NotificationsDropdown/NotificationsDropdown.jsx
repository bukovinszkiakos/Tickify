"use client";

import React, { useEffect, useState } from "react";
import { apiGet, apiPost } from "../utils/api";
import { useRouter } from "next/navigation";
import { Bell } from "lucide-react";
import "../styles/NotificationsDropdown.css";

export default function NotificationsDropdown() {
  const [notifications, setNotifications] = useState([]);
  const [isOpen, setIsOpen] = useState(false);
  const router = useRouter();

  useEffect(() => {
    loadNotifications();
  }, []);

  async function loadNotifications() {
    try {
      const data = await apiGet("/api/user/notifications");
      setNotifications(data);
    } catch (err) {
      console.error("Failed to load notifications:", err);
    }
  }

  const handleToggle = () => setIsOpen((prev) => !prev);

  const handleNotificationClick = async (notification) => {
    try {
      await apiPost(`/api/user/notifications/${notification.id}/read`, {});
  
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === notification.id ? { ...n, isRead: true } : n
        )
      );
  
      router.push(`/tickets/${notification.ticketId}`);
    } catch (err) {
      console.error("Error marking notification as read", err);
    }
  };
  

  return (
    <div className="notification-wrapper">
      <button className="notification-button" onClick={handleToggle}>
        <Bell />
        {notifications.some((n) => !n.isRead) && <span className="dot" />}
      </button>

      {isOpen && (
        <div className="notification-dropdown">
          <h4>Notifications</h4>
          {notifications.length === 0 ? (
            <p className="empty-text">No notifications</p>
          ) : (
            notifications.map((notification) => (
              <div
                key={notification.id}
                className={`notification-item ${
                  notification.isRead ? "read" : "unread"
                }`}
                onClick={() => handleNotificationClick(notification)}
              >
                {notification.message}
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}
