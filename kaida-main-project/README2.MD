# 🧭 Kaida Authentication & Access System — Project Design Document

**Author:** Amy  
**Project Type:** Personal/Portfolio Project  
**Goal:** Build a secure, central authentication server (AuthServer) that manages login and access to multiple connected applications (e.g., Dashboard, Trello clone, etc.).  
**Tech Stack:**  
- **Backend:** ASP.NET Core (.NET 8), EF Core, Identity  
- **Frontend:** Blazor Server (Dashboard Admin UI), later possibly Blazor WebAssembly or Ionic apps  
- **Database:** SQL Server (local dev) → PostgreSQL (prod)  
- **Auth Mechanism:** Centralized AuthServer issuing JWT tokens with app access claims  

---

## 🔒 1. Core Authentication Pattern

**Chosen Pattern:**  
**Hybrid centralized authentication.**  
A single **AuthServer** handles login and issues **JWT access tokens** and **refresh tokens**.  
Each connected app validates tokens locally and may optionally verify them with AuthServer for sensitive actions.

### Workflow Summary
1. User logs in via AuthServer (or embedded login UI hosted by AuthServer).  
2. AuthServer validates credentials and returns:
   - Short-lived **access token (JWT)** with claims for authorized apps.
   - Long-lived **refresh token** to renew access tokens.
3. User opens another connected app (e.g., Dashboard).  
4. The app:
   - Verifies JWT signature and expiration.
   - Checks whether its **AppId** exists in the token claims.
   - Grants or denies access accordingly.  
5. Apps may call AuthServer’s `/check-access` for an extra validation layer if needed.

---

## 🧱 2. Token Structure & Claims

| Claim     | Type        | Description |
|------------|-------------|--------------|
| `sub` | string | User ID (GUID or IdentityUser ID) |
| `email` | string | User’s email |
| `name` | string | Username |
| `appId` | array of GUIDs | Applications user has access to |
| `role` | string | Role or access level (Admin/User) |
| `iss` | string | Issuer (AuthServer) |
| `aud` | string | Audience (KaidaApps) |
| `iat` | timestamp | Issued at |
| `exp` | timestamp | Expires at |

### Example
```json
{
  "sub": "user-123",
  "email": "amy@example.com",
  "name": "Amy",
  "appId": ["9d89...f5", "8c22...a2"],
  "role": "Admin",
  "iss": "https://auth.kaida.local",
  "aud": "KaidaApps",
  "iat": 1731270000,
  "exp": 1731273600
}
```

---

## ⏱️ 3. Token Lifetimes

| Type | Lifetime | Storage | Purpose |
|------|-----------|----------|----------|
| **Access Token** | 15–60 min | In memory or encrypted session | Sent on every request |
| **Refresh Token** | 7–30 days | Secure HttpOnly cookie / DB | To renew access token |

- **Rotation:** New pair issued each refresh; old one invalidated.  
- **Revocation:** Refresh tokens tracked in DB for forced logout.  
- **Access tokens:** Not stored (stateless); optionally cached for quick invalidation.

---

## 🗃️ 4. Database Architecture

### ✅ **Architecture Decision: One Database Server, Multiple Databases**

All Kaida applications share **one database provider instance** (e.g., SQL Server or PostgreSQL).  
Each app has its **own isolated database** for security, maintainability, and future scalability.

| Database | Purpose |
|-----------|----------|
| `Kaida.AuthDb` | Authentication, users, refresh tokens, app registrations |
| `Kaida.DashboardDb` | Dashboard-specific data |
| `Kaida.TrelloDb` | Trello clone data |
| `Kaida.SharedDb` (optional) | Global logs or audit data |

Each app uses its own connection string and `DbContext`.

### Benefits
- ✅ Isolation between apps (migrations, schema, data).  
- ✅ Easier backups and restores.  
- ✅ Scalable to multi-server setup later.  
- ✅ More secure (different credentials per app).  
- ✅ Fits real-world microservice architecture principles.

### Example Connection Strings
`Kaida.AuthServer/appsettings.json`
```json
"ConnectionStrings": {
  "AuthConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Kaida.AuthDb;Trusted_Connection=True;"
}
```

`Kaida.Dashboard/appsettings.json`
```json
"ConnectionStrings": {
  "DashboardConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Kaida.DashboardDb;Trusted_Connection=True;"
}
```

---

## 📦 5. Entities Overview

**AuthServer Entities**

| Entity | Description |
|---------|--------------|
| `Application` | Defines each registered app with Id + Name |
| `UserAccess` | Links users to apps with access level |
| `RefreshToken` | Stores tokens, expiration, revocation flag |

**Each App’s Entities**
- Defined per app (`DashboardDbContext`, etc.)
- Only app-specific data, no authentication data.

---

## ⚙️ 6. AuthServer Endpoints

| Endpoint | Method | Description |
|-----------|---------|-------------|
| `/api/auth/login` | POST | Validates credentials, returns tokens |
| `/api/auth/refresh` | POST | Refreshes and rotates tokens |
| `/api/auth/check-access` | GET | Verifies if user has access to an AppId |
| `/api/admin/apps` | CRUD | Manage registered applications |
| `/api/admin/users` | CRUD | Manage users and assign app access |

---

## 🧭 7. App Integration Logic

Each Kaida app:
- Has an `AppId` configured in `appsettings.json`.
- Validates JWTs locally using AuthServer’s public key.
- Checks its own `AppId` claim before allowing access.
- Calls AuthServer’s `/refresh` or `/check-access` as needed.

---

## 🔐 8. Security Practices

- **HTTPS** only.
- **JWT signing:** RS256 preferred (asymmetric public/private key).
- Secrets stored via **User Secrets** (dev) and **environment variables** (prod).
- **Short-lived** access tokens, **rotating** refresh tokens.
- **HttpOnly cookies** preferred over `localStorage`.
- Validate signature, issuer, audience, and expiration.
- **Admin routes** protected with role-based authorization.
- Consider audit logging and IP-based token tracking.

---

## 🧑‍💼 9. Dashboard Admin UI

**Type:** Blazor Server  
**Purpose:** Central management UI for all Kaida applications.  

### Features
- Login via AuthServer.
- Manage apps, users, and access.
- View and revoke tokens.
- Audit login activity and roles.

### Security
- Only `"Admin"` claim users allowed.
- Uses AuthServer’s JWTs for authentication.

---

## 🧰 10. Development Setup

### Projects
| Project | Type | Description |
|----------|------|-------------|
| `Kaida.AuthServer` | ASP.NET Core API | Central authentication server |
| `Kaida.AuthServer.Tests` | xUnit | Unit tests for Auth logic |
| `Kaida.Dashboard` | Blazor Server | Admin dashboard |
| (Future) Other apps | API/UI | Authenticated via AuthServer |

### Database
- **Dev:** SQL Server LocalDB  
- **Prod:** PostgreSQL or Azure SQL  

Run sequence:
1. Run EF migrations per project.
2. Seed AuthDb (default admin + sample apps).
3. Start AuthServer, test `/login` and `/check-access`.
4. Connect Dashboard to AuthServer.

---

## 🧪 11. Testing

- **Unit Tests:** Login flow, access checks, refresh token rotation.  
- **Integration Tests:** Token validation across apps, revocation handling.

---

## 🔄 12. Future Enhancements

- External logins (Google, GitHub).  
- Two-Factor Authentication (2FA).  
- Full OIDC compliance (optional).  
- Audit and logging system.  
- API gateway (future scaling).

---

## ✅ 13. Key Design Principles

| Principle | Explanation |
|------------|--------------|
| Centralized Authentication | One identity source for all apps |
| Decentralized Validation | Each app validates JWTs locally |
| Least Privilege | Tokens carry only needed claims |
| Short Token Lifetimes | Minimize damage if stolen |
| Refresh Token Rotation | Prevent replay attacks |
| Isolated Databases | Security and maintainability |
| Extensible | Easy to add new apps later |
| Transparent Admin Control | Dashboard provides oversight |

---

## 🚀 14. Next Steps

1. Implement AuthDb + initial seeding.  
2. Build `AuthController` for `/login`, `/check-access`, `/refresh`.  
3. Set up Dashboard Blazor login flow with JWT verification.  
4. Implement role-based protection in Dashboard.  
5. Configure multiple DbContexts and connection strings.  
6. Write initial unit tests for Auth flow.  

---

**End of Document**
