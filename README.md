# 🛠️ Tickify

## 📌 About The Project

**Tickify – Full-Featured Ticketing System with Admin Control**

Tickify is a web-based ticket management platform that allows users to create and manage issue tickets, while admins and super admins handle them through a role-based system. It provides a robust structure for tracking, communicating, and resolving problems.

The project features a **Next.js (App Router)** frontend and an **ASP.NET Core Web API** backend with **JWT-based authentication**, role management, and email-style in-app notifications.

---

### ✨ Core Features

- ✅ User registration & login with JWT  
- ✅ Create, edit, delete tickets  
- ✅ File/image upload with ticket  
- ✅ Comment system with timestamp  
- ✅ Admin role to manage tickets  
- ✅ Super Admin role to manage users and roles  
- ✅ Role-based permissions  
- ✅ Status updates with logs (Open, Resolved, etc.)  
- ✅ Notifications for ticket replies and updates  
- ✅ Modern UI with animations (Lottie)  
- ✅ Responsive frontend with Next.js (App Router)

## 🎞️ Live Demo (GIF)

Below is a quick look at just a few key features of Tickify in action.

> ⏯️ These short demos highlight a single user flow, but Tickify offers much more – including full role-based access, detailed status tracking, admin dashboards, user management, and in-app notifications.

### 🧑‍💻 A user creates a new support ticket with a short description and an optional screenshot.
<img src="https://github.com/user-attachments/assets/72133e7a-72bf-4fc5-9d53-b6051cf184e9" width="100%" />

### 🛠️ An admin assigns the ticket to themselves and replies with helpful steps or attachments.
<img src="https://github.com/user-attachments/assets/9d11f5d7-0b91-4c87-ae1c-00ce24a728a7" width="100%" />

### ✅ Once resolved, the user checks the updated ticket and sees a notification.
<img src="https://github.com/user-attachments/assets/f3ecddd2-98ae-4d9b-bf93-b31615188586" width="100%" />


## 🔧 Built With

- [![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/en-us/)
- [![Next.js](https://img.shields.io/badge/Next.js-000000?style=for-the-badge&logo=nextdotjs&logoColor=white)](https://nextjs.org/)
- [![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://reactjs.org/)
- [![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
- [![Entity Framework Core](https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/)
- [![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
- [![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

---

## 🚀 Getting Started

### 1️⃣ Prerequisites

Make sure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [Node.js (v18+)](https://nodejs.org/)  
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### 2️⃣ Clone the repository

```bash
git clone https://github.com/your-username/tickify.git
cd tickify
```

---

### 3️⃣ Run with Docker (Optional)

```bash
docker compose up -d
```

> Starts:
> - SQL Server  
> - ASP.NET Core backend (API)  
> - Next.js frontend

---

## 🔐 Authentication & Roles

Tickify uses **JWT tokens** and supports 3 user roles:

- 👤 **User** – Create and manage personal tickets  
- 🛠️ **Admin** – Manage and respond to all tickets  
- 👑 **SuperAdmin** – Admin privileges + user role management

---

## 🔔 Notifications

Users and Admins receive real-time style in-app notifications when:

- A ticket is updated  
- A new comment is added  
- The ticket status changes  
- A reply is received from admin/user

---

## 🧪 Testing

Coming soon...

---

## 📅 Roadmap

### ✅ Completed

- [x] Authentication & role system  
- [x] Ticket CRUD operations  
- [x] Commenting on tickets  
- [x] In-app notification dropdown (🔔)  
- [x] SuperAdmin user management  
- [x] Role-based access control  

### 🚧 Planned

- [ ] Email notifications  
- [ ] Notification center with pagination  
- [ ] Assign tickets to specific admins  
- [ ] Search tickets by keyword  
- [ ] Mobile-friendly design  

---

## 👨‍💻 Developer

- **Ákos Bukovinszki**  
  [GitHub Profile](https://github.com/bukovinszkiakos)

---

## 🛡️ License

This project is licensed under the MIT License.

---

## 📬 Contact

📂 Repository: [https://github.com/bukovinszkiakos/Tickify.git](https://github.com/bukovinszkiakos/Tickify)

---

<p align="right">(<a href="#top">Back to top</a>)</p>




