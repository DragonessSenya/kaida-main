
# 🧭 Kaida Authentication & Access System — Project Design Document

**Author:** Amy  
**Project Type:** Personal/Portfolio Project  
**Goal:** Build a secure, central authentication server (AuthServer) that manages login and access for multiple connected applications (e.g., Dashboard, Trello clone, etc.).  

**Tech Stack:**  
- **Backend:** ASP.NET Core (.NET 8), EF Core, Identity  
- **Frontend:** Blazor Server (Dashboard Admin UI), later possibly Blazor WebAssembly or Ionic apps  
- **Database:** SQL Server LocalDB / SQLite (dev) → PostgreSQL / Azure SQL (prod)  
- **Auth Mechanism:** Centralized AuthServer issuing JWT tokens with app access claims  

---

## 🔒 1. Core Authentication Pattern

**Pattern:** **Hybrid centralized authentication**  
- A single **AuthServer** handles login and issues **JWT access tokens** and **refresh tokens**.  
- Each connected app validates tokens locally but may optionally verify with AuthServer for sensitive actions.  

### Workflow Summary
1. User logs in via AuthServer (or embedded login UI).  
2. AuthServer validates credentials and returns:
   - Short-lived **access token (JWT)** containing claims for authorized apps.
   - Long-lived **refresh token** for renewing access tokens.  
3. User opens a connected app (e.g., Dashboard).  
4. The app:
   - Verifies JWT signature and expiration.
   - Checks if its **AppId** exists in token claims.
   - Grants or denies access accordingly.  
5. Apps may call AuthServer’s `/check-access` endpoint for extra validation.  

---

## 🧱 2. Token Structure & Claims

| Claim     | Type        | Description |
|-----------|------------|-------------|
| `sub`     | string     | User ID (GUID or IdentityUser ID) |
| `email`   | string     | User’s email |
| `name`    | string     | Username |
| `appId`   | array of GUIDs | List of applications user has access to |
| `appName` | array of strings (optional) | Readable app names for admin/debug |
| `role`    | string     | Role or access level (Admin/User) |
| `iss`     | string     | Issuer (AuthServer URL) |
| `aud`     | string     | Audience (KaidaApps) |
| `iat`     | timestamp  | Issued at |
| `exp`     | timestamp  | Expiry |

### Example JWT Payload
```json
{
  "sub": "user-123",
  "email": "amy@example.com",
  "name": "Amy",
  "appId": ["9d89...f5", "8c22...a2"],
  "appName": ["Dashboard", "KaidaTrello"],
  "role": "Admin",
  "iss": "https://auth.kaida.local",
  "aud": "KaidaApps",
  "iat": 1731270000,
  "exp": 1731273600
}
```

---

## ⏱️ 3. Token Lifetimes

| Token Type        | Lifetime      | Storage                          | Purpose                      |
|------------------|---------------|---------------------------------|-------------------------------|
| **Access Token**  | 15–60 min     | In memory or encrypted session  | Sent on each API call         |
| **Refresh Token** | 7–30 days     | Secure HttpOnly cookie or DB    | Renew access tokens           |

- **Rotation:** New access + refresh pair issued on refresh; old refresh token invalidated.  
- **Revocation:** Refresh tokens tracked in DB with revocation flags.  
- **Access Tokens:** Stateless, optionally cached for quick revocation.  

---

## 🗃️ 4. Database Architecture

### Architecture Decision
**One database provider, multiple databases per app**  

- **AuthServer** and all apps share a provider (SQL Server / PostgreSQL) but use **isolated databases**:
  
| Database        | Purpose |
|----------------|---------|
| `Kaida.AuthDb` | AuthServer: users, refresh tokens, app registrations |
| `Kaida.DashboardDb` | Dashboard-specific data |
| `Kaida.TrelloDb` | Trello clone data |
| `Kaida.SharedDb` (optional) | Global logs or audit data |

- Each app has its own connection string and DbContext for isolation.  
- Benefits: security, maintainability, scalability, easier backups.  

### Example Connection Strings
`Kaida.AuthServer/appsettings.json`
```json
"ConnectionStrings": {
  "AuthConnection": "Server=(localdb)\MSSQLLocalDB;Database=Kaida.AuthDb;Trusted_Connection=True;"
}
```

`Kaida.Dashboard/appsettings.json`
```json
"ConnectionStrings": {
  "DashboardConnection": "Server=(localdb)\MSSQLLocalDB;Database=Kaida.DashboardDb;Trusted_Connection=True;"
}
```

---

## 📦 5. Entities Overview

### AuthServer Entities
| Entity | Description |
|--------|------------|
| `Application` | Registered apps with Id + Name |
| `UserAccess` | Maps users to apps with access levels |
| `RefreshToken` | Stores refresh tokens with expiry and revocation |

### Each App’s Entities
- App-specific data only (`DashboardDbContext`, etc.)
- No auth-related data stored locally  

---

## ⚙️ 6. AuthServer API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/login` | POST | Validates credentials; returns JWT + refresh token |
| `/api/auth/refresh` | POST | Rotates refresh token and returns new JWT + refresh token |
| `/api/auth/check-access` | GET | Verifies user access to a specific AppId |
| `/api/admin/apps` | CRUD | Manage registered applications |
| `/api/admin/users` | CRUD | Manage users and app access |

---

## 🧭 7. App Integration Logic

Each connected app must:
- Configure its **AppId** in `appsettings.json`.  
- Validate JWTs locally using AuthServer’s public key.  
- Ensure its AppId exists in token claims.  
- Call `/refresh` or `/check-access` if token expired or extra verification needed.  

---

## 🔐 8. Security Practices

- **HTTPS only**  
- JWT Signing: RS256 (asymmetric) preferred; secrets stored securely  
- **Short-lived access tokens**, **rotating refresh tokens**  
- **HttpOnly cookies** or server memory for tokens (avoid localStorage)  
- Validate signature, issuer, audience, expiration  
- Admin routes protected with **role-based authorization**  
- Optional: audit logging, IP tracking, token revocation cache  

---

## 🧑‍💼 9. Dashboard Admin UI

**Type:** Blazor Server  
**Purpose:** Central management UI for all Kaida apps  

### Features
- Login via AuthServer  
- Manage apps, users, and access  
- View and revoke tokens  
- Audit login activity and roles  

### Security
- Only `"Admin"` role users allowed  
- Uses AuthServer JWTs for authentication  

---

## 🧰 10. Development Setup

### Projects
| Project | Type | Description |
|---------|------|------------|
| `Kaida.AuthServer` | ASP.NET Core API | Central auth service |
| `Kaida.AuthServer.Tests` | xUnit | Unit tests for auth logic |
| `Kaida.Dashboard` | Blazor Server | Admin dashboard |
| (Future) Other apps | API/UI | Authenticated via AuthServer |

### Database
- **Dev:** SQL Server LocalDB or SQLite  
- **Prod:** PostgreSQL or Azure SQL  

### Run Sequence
1. Run EF migrations per database/project  
2. Seed AuthDb with default admin user and sample apps  
3. Start AuthServer and test `/login` and `/check-access`  
4. Connect Dashboard via HttpClient  

---

## 🧪 11. Testing

- **Unit Tests:** Login, access checks, refresh token rotation  
- **Integration Tests:** Token validation across apps, revocation handling  

---

## 🔄 12. Future Enhancements

- External logins (Google, GitHub)  
- Two-Factor Authentication (TOTP or email)  
- Full OpenID Connect compliance  
- Audit logging and token tracking  
- API gateway for multi-app scaling  

---

## ✅ 13. Key Design Principles

| Principle | Explanation |
|-----------|------------|
| Centralized Authentication | One identity source for all apps |
| Decentralized Validation | Each app validates JWTs locally |
| Least Privilege | Tokens include only necessary claims |
| Short-lived Access Tokens | Minimize damage if token is stolen |
| Refresh Token Rotation | Prevent replay attacks |
| Isolated Databases | Security, maintainability, scalability |
| Extensible | Easy to add new apps or claims |
| Transparent Admin Control | Dashboard gives oversight |

---

## 🚀 14. Next Steps

1. Define final JWT claim structure and implement in `AuthController`  
2. Implement refresh token storage, rotation, and endpoints  
3. Set up Dashboard login and role-based route protection  
4. Configure multiple DbContexts and connection strings  
5. Write unit and integration tests for authentication flows  
6. Seed data with default admin and example applications  

---

**End of Document**
