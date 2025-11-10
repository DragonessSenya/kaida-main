# 🧭 Kaida Authentication & Access System — Project Design Document

**Author:** Amy  
**Project Type:** Personal/Portfolio Project  
**Goal:** Build a secure, central authentication server (AuthServer) that manages login and access to multiple connected applications (e.g., Dashboard, Trello clone, etc.).  
**Tech Stack:**  
- **Backend:** ASP.NET Core (.NET 8), EF Core, Identity  
- **Frontend:** Blazor Server (Dashboard Admin UI), later possibly Blazor WebAssembly or Ionic apps  
- **Database:** SQLite (dev) → PostgreSQL (prod)  
- **Auth Mechanism:** Centralized AuthServer issuing JWT tokens with app access claims  

---

## 🔒 1. Core Authentication Pattern

**Chosen Pattern:**  
**Hybrid centralized authentication.**  
A single **AuthServer** handles login and issues **JWT access tokens** and **refresh tokens**.  
Each connected app verifies the token locally but can also contact the AuthServer for access checks or token refresh.

### Workflow Summary:
1. User logs in via AuthServer (or via embedded login page hosted by AuthServer).  
2. AuthServer verifies credentials and returns:
   - Short-lived **access token (JWT)** containing claims about user and authorized apps.
   - Long-lived **refresh token** to renew the access token.
3. The user navigates to any connected app.
4. Each app:
   - Verifies the JWT signature and expiry.
   - Checks if its **AppId** exists in the user’s token claims.
   - If valid → grants access; if not → denies.
5. Optionally, the app can contact `/check-access` on AuthServer to double-verify.

---

## 🧱 2. Token Structure & Claims

Each issued JWT includes the following **claims**:

| Claim     | Type                        | Description                                 |
|-----------|-----------------------------|---------------------------------------------|
| `sub`     | string                      | User ID (GUID or IdentityUser ID)           |
| `email`   | string                      | User’s email address                        |
| `name`    | string                      | Username                                    |
| `appId`   | array of GUIDs              | List of applications the user has access to |
| `appName` | array of strings (optional) | Readable app names for admin/debug          |
| `iat`     | timestamp                   | Issued at                                   |
| `exp`     | timestamp                   | Expiry                                      |
| `iss`     | string                      | Issuer (AuthServer URL)                     |
| `aud`     | string                      | Audience (App IDs or general “KaidaApps”)   |

### Example JWT payload:
```json
{
  "sub": "user-123",
  "email": "amy@example.com",
  "name": "Amy",
  "appId": ["9d89...f5", "8c22...a2"],
  "appName": ["Dashboard", "KaidaTrello"],
  "iss": "https://auth.kaida.local",
  "aud": "KaidaApps",
  "iat": 1731270000,
  "exp": 1731273600
}
```

---

## ⏱️ 3. Token Lifetimes

| Token Type        | Lifetime      | Storage                          | Purpose                      |
|-------------------|---------------|----------------------------------|------------------------------|
| **Access Token**  | 15–60 minutes | In memory (or encrypted session) | Sent on each API call        |
| **Refresh Token** | 7–30 days     | Secure HttpOnly cookie or DB     | Used to get new access token |

**Rotation:**  
When a refresh token is used, AuthServer issues a new pair (access + refresh) and invalidates the old refresh token.  
**Revocation:**  
Refresh tokens stored server-side with flags for revocation.  
Access tokens not stored (short-lived); optional in-memory revocation cache.

---

## 🧮 4. Database & Entities

**Database:** EF Core Code First

### Entities
#### Application
| Property    | Type         | Description      |
|-------------|--------------|------------------|
| Id          | `Guid`       | Unique AppId     |               
| Name        | `string`     | App display name |
| Description | `string?`    | Optional info    |

#### UserAccess
| Property    | Type       | Description                 |
|-------------|------------|-----------------------------|
| Id          | `int`      | Identity                    |
| UserId      | `string`   | Foreign key to IdentityUser |
| AppId       | `Guid`     | Foreign key to Application  |
| AccessLevel | `string`   | e.g. Admin, User            |
| CreatedAt   | `DateTime` | Audit trail                 |

#### RefreshToken
| Property   | Type       | Description          |
|------------|------------|----------------------|
| Id         | `int`      | Identity             |
| Token      | `string`   | Secure random string |
| UserId     | `string`   | Foreign key          |
| ExpiryDate | `DateTime` | Expiration           |
| IsRevoked  | `bool`     | Revocation flag      |
| CreatedAt  | `DateTime` | Audit trail          |

---

## ⚙️ 5. AuthServer API Endpoints

### `/api/auth/login`
- **POST**
- Accepts `username`, `password`.
- Verifies credentials.
- Loads all apps the user has access to.
- Returns:
  ```json
  {
    "token": "<jwt>",
    "refreshToken": "<refresh_token>",
    "expiration": "<datetime>"
  }
  ```

### `/api/auth/refresh`
- **POST**
- Accepts a refresh token.
- Validates + rotates token.
- Returns a new access + refresh token pair.

### `/api/auth/check-access`
- **GET (Authorized)**
- Checks if the user’s token contains the given `AppId`.
- Returns `{ "hasAccess": true/false }`.

### `/api/admin/apps`
- CRUD endpoints for managing registered applications (Admin only).

### `/api/admin/users`
- Manage user accounts, assign/revoke app access (Admin only).

---

## 🧭 6. App Integration Logic

Each app (e.g., Dashboard, Trello clone, etc.) must have:
- Its own **AppId (GUID)** configured in `appsettings.json`.
- A middleware to:
  1. Validate the JWT.
  2. Ensure its AppId is in the token’s list of `appId` claims.

If the token is expired, the app uses `/auth/refresh` to renew.

If user’s access is revoked, refresh token rotation + short access token lifetime ensures the user loses access quickly.

---

## 🔐 7. Security Practices

- Use **HTTPS** for all communication.
- JWT signing:
  - **Algorithm:** RS256 (asymmetric) or HS256 with ≥ 32-byte secret.
  - Key stored securely (User Secrets in dev, environment vars in prod).
- Access tokens **short-lived** (reduce risk of theft).
- Refresh tokens **rotated & revocable**.
- Never store JWTs in `localStorage`; use **HttpOnly cookies** or server memory.
- Validate:
  - Signature
  - Issuer
  - Audience
  - Expiration
- Implement a **revocation list** (cache or DB) if critical.
- Limit sensitive endpoints (e.g., `/admin`) via roles/claims.

---

## 🧑‍💼 8. Dashboard Admin UI

**Type:** Blazor Server  
**Purpose:** Manage apps, users, and their access.

### Features:
- Login via AuthServer.
- Show overview of registered apps.
- CRUD for Applications.
- CRUD for UserAccess entries.
- Option to revoke tokens.
- Visualize login activity and token expirations.

### Security:
- Only users with `"Admin"` role claim can access this UI.
- Uses the same central AuthServer tokens for authentication.

---

## 🧰 9. Development Setup

### Projects
| Project                  | Type          | Description                    |
|--------------------------|---------------|--------------------------------|
| `Kaida.AuthServer`       | ASP.NET API   | Central auth service           |
| `Kaida.AuthServer.Tests` | xUnit         | Unit tests                     |
| `Kaida.Dashboard`        | Blazor Server | Admin UI                       |
| (Future) Other apps      | API/UI        | Authenticated using AuthServer |

### Database
- Dev: SQLite (`Data Source=kaida.db`)
- Prod: PostgreSQL (`ConnectionString` via env vars)

### Run Sequence
1. Run migrations: `dotnet ef database update`
2. Seed database:
   - Default admin user
   - Example apps
3. Start AuthServer.
4. Test via Swagger or Postman:
   - `/api/auth/login`
   - `/api/auth/check-access`
5. Connect Dashboard to AuthServer via HttpClient.

---

## 🧪 10. Testing

- **Unit tests:**
  - Login success/fail
  - Access check logic
  - Refresh token rotation
- **Integration tests:**
  - Token validation pipeline
  - JWT claim parsing in downstream apps

---

## 🔄 11. Future Enhancements

- Support external logins (Google, GitHub) through IdentityServer if needed.
- Add MFA (2FA) via TOTP or email verification.
- Implement full OIDC (OpenID Connect) for standards compliance.
- Add audit logging for token issuance and revocation.

---

## ✅ 12. Key Design Principles

- **Centralized authentication**: one identity source for all apps.  
- **Decentralized validation**: each app can independently validate tokens.  
- **Least privilege**: token includes only what’s necessary.  
- **Short-lived access tokens**: minimize damage from leaks.  
- **Refresh token rotation**: prevent replay attacks.  
- **Extensible**: new apps and scopes can be added easily.  
- **Transparency**: Admin Dashboard gives visibility and control.

---

## 🚀 13. Next Steps

1. Define final **JWT claim structure** (implement in AuthController).  
2. Implement **refresh token storage & endpoint**.  
3. Connect **Blazor Dashboard** to AuthServer (admin login + CRUD for apps/users).  
4. Secure Dashboard routes with role-based authorization.  
5. Add `/check-access` and token verification middleware for other apps.  
6. Add integration tests and seed data.

---

**End of Document**
