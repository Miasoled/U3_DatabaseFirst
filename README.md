# SakilaApp

ASP.NET Core MVC application built on top of the Sakila database (PostgreSQL). Demonstrates a complete integration of ASP.NET Core Identity with Google OAuth, Gmail SMTP email confirmation, Multi-Factor Authentication (MFA), role-based authorization, and Entity Framework Core.

---

## Table of Contents

- [Technologies](#technologies)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Database Setup](#database-setup)
- [User Secrets Configuration](#user-secrets-configuration)
- [Running the Application](#running-the-application)
- [Seeded Accounts](#seeded-accounts)
- [Role Permissions](#role-permissions)
- [Project Structure](#project-structure)
- [Screenshots](#screenshots)

---

## Technologies

| Technology | Version |
|---|---|
| .NET | 10.0 |
| ASP.NET Core MVC | 10.0 |
| Entity Framework Core | 10.0.2 |
| Npgsql EF Core (PostgreSQL) | 10.0.1 |
| ASP.NET Core Identity | 10.0.2 |
| MailKit (Gmail SMTP) | 4.17.0 |
| Microsoft.AspNetCore.Authentication.Google | 10.0.2 |
| Bootstrap | 5.x |
| Bootstrap Icons | 1.11.3 |

---

## Features

- **Local account registration** with mandatory email confirmation
- **Google OAuth** login (sign in with Google account)
- **Email confirmation** sent via Gmail SMTP using an Application Password
- **Password recovery** via email verification link
- **Two-Factor Authentication (MFA)** using Microsoft Authenticator or Google Authenticator (TOTP)
- **Role-based authorization** with `Administrador` and `Empleado` roles
- **Protected controllers** — different access levels per role
- **Admin panel** with role permission overview
- **Full Sakila catalog** — Films, Actors, Categories, Languages with search and pagination

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- A Gmail account with an [App Password](https://myaccount.google.com/apppasswords) generated
- A [Google Cloud Console](https://console.cloud.google.com/) project with OAuth 2.0 credentials

---

## Database Setup

### 1. Restore the Sakila database

Open pgAdmin or use `psql` to restore the included backup file:

```bash
psql -U postgres -c "CREATE DATABASE sakila;"
pg_restore -U postgres -d sakila sakila.backup
```

Or via pgAdmin:
1. Right-click **Databases** → **Create** → **Database** → name it `sakila`
2. Right-click the `sakila` database → **Restore** → select `sakila.backup`

### 2. Create the database user

```sql
CREATE USER cruduser WITH PASSWORD 'crud123';
GRANT ALL PRIVILEGES ON DATABASE sakila TO cruduser;
GRANT ALL ON SCHEMA public TO cruduser;
```

### 3. Apply Identity migrations

The Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) are created automatically on first run via EF Core migrations. If they are not present, run:

```bash
dotnet ef database update
```

---

## User Secrets Configuration

Sensitive configuration values are stored using the .NET User Secrets manager and are **not** committed to source control.

Initialize user secrets (only needed once):

```bash
cd SakilaApp
dotnet user-secrets init
```

Set the required secrets:

```bash
# Google OAuth credentials (from Google Cloud Console)
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"

# Gmail SMTP settings
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SenderName" "SakilaApp"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "YOUR_GMAIL_APP_PASSWORD"
```

> **Note:** The Gmail password must be an **App Password**, not your regular Gmail password.  
> Generate one at: [https://myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)

To verify all secrets are set correctly:

```bash
dotnet user-secrets list
```

---

## Running the Application

```bash
cd SakilaApp
dotnet run
```

The application will be available at `https://localhost:5051` (or the port shown in the terminal).

On startup, the application automatically seeds the database with the default roles and admin/employee accounts (see [Seeded Accounts](#seeded-accounts)).

---

## Seeded Accounts

The following accounts are created automatically on first run via `IdentitySeeder.cs`:

| Email | Password | Role |
|---|---|---|
| `admin@espe.edu.ec` | `Admin123*` | Administrador |
| `empleado@espe.edu.ec` | `Empleado123*` | Empleado |

> Both accounts have `EmailConfirmed = true` so they can log in immediately without going through the email confirmation flow.

---

## Role Permissions

| Resource | Administrador | Empleado |
|---|---|---|
| Admin Panel (`/Home/PanelAdministrador`) | ✅ Full access | ❌ No access |
| Films — View & Edit (`/Films`) | ✅ Full access | ❌ No access |
| Actors — View (`/Actors/Index`, `/Actors/Details`) | ✅ | ✅ |
| Actors — Create / Edit / Delete | ✅ | ❌ |
| Categories (`/Categories`) | ✅ Full access | ❌ No access |
| Languages — View (`/Languages/Index`, `/Languages/Details`) | ✅ | ✅ |
| Languages — Create / Edit / Delete | ✅ | ❌ |
| Home, Privacy | ✅ Public | ✅ Public |

---

## Project Structure

```
SakilaApp/
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/          # Login, Register, ForgotPassword, etc.
│               └── Manage/       # Profile, Email, Password, 2FA, etc.
├── Controllers/
│   ├── HomeController.cs         # Index, Privacy, PanelAdministrador [Authorize(Roles="Administrador")]
│   ├── FilmsController.cs        # [Authorize(Roles="Administrador")]
│   ├── ActorsController.cs       # [Authorize] + write actions [Authorize(Roles="Administrador")]
│   ├── CategoriesController.cs   # [Authorize(Roles="Administrador")]
│   ├── LanguagesController.cs    # [Authorize(Roles="Administrador,Empleado")] + write [Authorize(Roles="Administrador")]
│   ├── FilmActorsController.cs
│   └── FilmCategoriesController.cs
├── Data/
│   ├── ApplicationDbContext.cs   # IdentityDbContext for ASP.NET Core Identity
│   ├── SakilaContext.cs          # EF Core context for Sakila database
│   └── IdentitySeeder.cs         # Seeds roles and default users on startup
├── Models/                       # EF Core entity models (Sakila schema)
├── Services/
│   └── GmailEmailSender.cs       # IEmailSender implementation using MailKit + Gmail SMTP
├── Settings/
│   └── EmailSettings.cs          # POCO config model for SMTP settings
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml        # Main layout with navbar and Bootstrap Icons
│   │   └── _LoginPartial.cshtml  # Auth nav items
│   ├── Home/
│   ├── Actors/
│   ├── Films/
│   ├── Categories/
│   ├── Languages/
│   ├── FilmActors/
│   └── FilmCategories/
├── wwwroot/
│   └── css/site.css              # Custom light neutral theme
├── appsettings.json              # Non-sensitive configuration
├── Program.cs                    # App startup, DI registration, middleware pipeline
└── SakilaApp.csproj
```

---

## Screenshots

Screenshots demonstrating the running application are located in the `/screenshots` folder of this repository:

- Google Cloud Console OAuth configuration
- Gmail SMTP App Password setup
- User Secrets configuration
- User registration and email confirmation
- Google login flow
- Password recovery
- Two-Factor Authentication setup and verification
- PostgreSQL Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserLogins`, `AspNetUserTokens`)
- Protected routes (Admin Panel, Films, Categories)
- Administrator and Employee role demonstration
- Application running successfully

---

## Database Backup

The PostgreSQL backup file is included in this repository as `sakila.backup`.  
Restore instructions are in the [Database Setup](#database-setup) section above.
