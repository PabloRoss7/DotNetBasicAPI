# User Management API

A RESTful API built with ASP.NET Core for managing users. The project was created to practice common backend development concepts used in modern web applications.

## Features

* Full CRUD operations for user management:
  * Create users
  * Retrieve users
  * Update users
  * Delete users
* Input validation using Data Annotations and model validation
* JWT-based authentication and authorization
* Rate limiting to protect the API from excessive requests
* Custom middleware for request logging and request processing
* Interactive API documentation with OpenAPI (Scalar UI)

## Tech Stack

* .NET 10 / ASP.NET Core (Minimal Hosting Model, Controllers)
* OpenAPI via `Microsoft.AspNetCore.OpenApi`, rendered with Scalar
* JWT authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)

### First-time setup (development)

The JWT signing key is **not** stored in the repo. Set it once via user-secrets:

```bash
dotnet user-secrets set "Jwt:Key" "any-long-dev-key-at-least-32-characters" --project DotNetBasicAPI
```

Without this, the app can't sign tokens and login will fail.

### Run

From the solution root (`UserManagementApi/`):

```bash
dotnet run --project DotNetBasicAPI --launch-profile http
```

The API listens on `http://localhost:5095`.

* Interactive docs (Scalar UI): `http://localhost:5095/scalar/v1`
* OpenAPI document: `http://localhost:5095/openapi/v1.json`

> Docs endpoints are only mapped in the Development environment.

### Configuration

JWT settings live under the `Jwt` section (`Issuer`, `Audience`, `ExpiryMinutes` in `appsettings.json`). The **signing key is a secret and is never committed**:

* **Development** → `dotnet user-secrets` (see setup above).
* **Production** → an environment variable `Jwt__Key` (double underscore maps to `Jwt:Key`).

Configuration is layered: `appsettings.json` → `appsettings.{Environment}.json` → user-secrets (dev) → environment variables. Later sources override earlier ones.

### Running in production mode (locally)

In production the app runs the published DLL (not `dotnet run`), with `ASPNETCORE_ENVIRONMENT=Production` and the signing key injected as an environment variable. `appsettings.Production.json` applies (quieter logging, higher rate limit) and the docs endpoints are not mapped.

**Option A — published DLL** (run from the solution root):

```powershell
dotnet publish DotNetBasicAPI/DotNetBasicAPI.csproj -c Release -o publish
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:Jwt__Key = "any-long-production-key-at-least-32-characters"
dotnet publish/DotNetBasicAPI.dll
```

**Option B — Docker** (requires Docker installed; build from the solution root):

```bash
docker build -t usermgmt-api .
docker run -p 8080:8080 -e Jwt__Key="any-long-production-key-at-least-32-characters" usermgmt-api
```

The container listens on port `8080`. In both cases the JWT key comes from the environment, never from a file in the repo.

## Endpoints

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Public | Register a new user (name, email, password) |
| POST | `/api/auth/login` | Public | Log in and receive a JWT |
| GET | `/api/users` | Public | List all users |
| GET | `/api/users/{id}` | Public | Get a user by id |
| POST | `/api/users` | Bearer | Create a user |
| PUT | `/api/users/{id}` | Bearer | Update a user |
| DELETE | `/api/users/{id}` | Bearer | Delete a user |

All endpoints are globally rate limited to 10 requests per 10 seconds per client IP; exceeding the limit returns `429 Too Many Requests`. User data is stored in memory, so it resets on restart.

### Design note: two paths to create a user

Users can be created in two ways: through `POST /api/auth/register` (public sign-up, which sets a password) and through `POST /api/users` (the CRUD create endpoint, which manages name/email only). This overlap is intentional: the project is built for the **"Desarrollo back-end con .NET" (Coursera)** course, whose requirements include exposing full CRUD operations over the `Users` resource. The registration endpoint is what authentication needs, while the CRUD `Create` is kept to satisfy that requirement, so both coexist by design rather than by oversight.

## Authentication flow

Write endpoints require a JWT in the `Authorization` header. To obtain one:

1. **Register** a user:

   ```http
   POST /api/auth/register
   Content-Type: application/json

   { "name": "Alice", "email": "alice@example.com", "password": "supersecret" }
   ```

2. **Log in** to get a token:

   ```http
   POST /api/auth/login
   Content-Type: application/json

   { "email": "alice@example.com", "password": "supersecret" }
   ```

   Response:

   ```json
   { "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...", "expiresAtUtc": "..." }
   ```

3. **Call a protected endpoint** with the token:

   ```http
   POST /api/users
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   Content-Type: application/json

   { "name": "Bob", "email": "bob@example.com" }
   ```

You can also paste the token into the Scalar UI to try protected endpoints from the browser. The `DotNetBasicAPI.http` file contains ready-to-run requests for the full flow.

## Concepts Practiced

* REST API design
* ASP.NET Core Minimal Hosting Model
* Controllers and routing
* Dependency Injection
* Middleware pipeline
* Authentication and Authorization with JWT
* Request validation
* HTTP status codes and error handling
* Rate limiting
* OpenAPI documentation

## Goal

The main goal of this project is to gain hands-on experience building secure and maintainable ASP.NET Core APIs while applying industry-standard backend development practices.
