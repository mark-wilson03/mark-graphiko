# MARK GRAPHIKO | Professional Portfolio & Client Engagement Platform

**MARK GRAPHIKO** is a full-stack web application designed for a graphic design studio specializing in streetwear apparel mockups, brand identities, typography systems, and vector illustrations.

---

## 🚀 Key Features & Architectural Highlights

### 1. Technology Stack
- **Backend Framework**: C# / ASP.NET Core 9.0 MVC
- **Database & ORM**: Entity Framework Core 9.0 (Code-First) supporting both **Microsoft SQL Server** (Production) and **SQLite** (Development / Zero-config local execution)
- **Identity & Security**: ASP.NET Core Identity with password hashing, roles (`Admin`, `User`), and cookie management
- **Social OAuth Providers**: Google OAuth & Facebook Login integration
- **Email Delivery Service**: SendGrid API + standard SMTP client with automatic local development outbox simulation
- **Frontend & Design System**: Modern HTML5, Tailwind CSS with class-based Dark/Light mode, custom CSS variables, and native JavaScript for micro-interactions and animations

---

### 2. UI/UX & Responsive Layout
- **Aesthetic**: Luxury obsidian dark mode paired with a clean minimalist light mode, glassmorphism blur panels, and typographic hierarchy using *Outfit* and *Space Grotesk*.
- **Adaptive Navigation**:
  - **Desktop View (>= 768px)**: Navigation bar is fixed at the **TOP** of the screen (`HOME | ABOUT | DESIGNS | MESSAGE | PROFILE / ADMIN`).
  - **Mobile & Tablet View (< 768px)**: Navigation bar shifts dynamically to the **BOTTOM** of the viewport for a native mobile app feel.
- **Theme Toggling**: Seamless light/dark mode switch persisted in `localStorage`.
- **Micro-Animations & Interactions**: Smooth scroll reveal triggered by `IntersectionObserver`, hover depth on artwork cards, image zoom, and interactive carousels with swipe gestures.
- **High-Resolution Artwork Modal**: Clicking any artwork opens a detail overlay with high-resolution preview, client info, software tools, and a direct *"Inquire About This Style"* button that pre-populates the inquiry form.

---

### 3. User Roles & Capabilities

#### A. Guest / Standard User View
1. **HOME**:
   - Hero banner with brand tagline and live commission status badge.
   - Interactive carousel featuring curated apparel mockups, vector artwork, and logos with touch-swipe, autoplay, and indicator dots.
   - Core competency pillars (Apparel, Logos, Vectors) and social proof stats.
2. **ABOUT**:
   - Studio backstory, design philosophy, and proficiency breakdown for tools: **Photoshop (98%)**, **Illustrator (99%)**, **Figma (92%)**, **CapCut / Premiere (88%)**.
   - 4-step creative workflow (Discovery &rarr; Vector Drafting &rarr; 3D Mockup Fitting &rarr; Factory Handoff).
3. **DESIGNS (Gallery)**:
   - Real-time instant search across titles, descriptions, clients, and tags.
   - Category filtering (All, T-Shirt Mockup, Logo Design, Vector Art, Branding & Identity, Typography).
   - AJAX-powered bookmarking/favoriting with persistent user collection.
4. **MESSAGE (Client Inquiry / Contact)**:
   - **Client Engagement / Security Rule**: Users must sign in or register (via Google, Facebook, or Email) prior to submitting a project brief.
   - Automated welcome/login notification email triggered upon connection.
   - Comprehensive inquiry brief form with project category selector, budget range, timeline, and optional reference design ID.
   - Automated confirmation email sent to the client and high-priority alert sent to the admin.
5. **PROFILE**:
   - Personal account settings, bio editor, saved bookmarked designs grid, and history of submitted inquiries with admin replies.

#### B. Admin View (`admin@markgraphiko.com`)
1. **DASHBOARD (`/Admin`)**:
   - Live analytics: Total artworks, categories, client inquiries, unread briefs, and registered user accounts.
2. **DESIGNS UPLOAD PANEL (`/Admin/Upload`)**:
   - High-resolution image file upload with real-time browser preview.
   - Category selection, title, description, client attribution, tools used, search tags, and `IsFeatured` carousel toggle.
   - Instant publication to public gallery and homepage carousels.
3. **INBOX & MESSAGING (`/Admin/Messages`)**:
   - Filter inquiries by All or Unread.
   - View client contact details (Email, Phone, Project Type, Budget).
   - Direct inline reply form that updates inquiry status and sends an automated reply email to the client.

---

## 🗄️ Database Schema (EF Core Code-First)

- **`ApplicationUser`** (extends `IdentityUser`):
  - `FullName`, `AvatarUrl`, `Bio`, `RegistrationProvider`, `CreatedAt`, `Favorites`, `Messages`
- **`Category`**:
  - `Id`, `Name`, `Slug`, `Description`, `DisplayOrder`, `DesignPosts`
- **`DesignPost`**:
  - `Id`, `Title`, `Description`, `CategoryId`, `ImagePath`, `ThumbnailPath`, `Client`, `ToolsUsed`, `Tags`, `IsFeatured`, `ViewCount`, `CreatedAt`
- **`Message`**:
  - `Id`, `SenderId`, `SenderName`, `SenderEmail`, `PhoneNumber`, `Subject`, `Content`, `ProjectType`, `BudgetRange`, `SentAt`, `IsRead`, `AdminReply`, `RepliedAt`
- **`FavoriteDesign`** (Composite Key: `UserId` + `DesignPostId`):
  - `UserId`, `DesignPostId`, `FavoritedAt`

---

## ⚙️ Configuration & Quick Start

### 1. Default Seed Accounts
Upon launching the application, the database is automatically created and seeded with default roles, categories, and test accounts:
- **Admin Account**:
  - **Email**: `admin@markgraphiko.com`
  - **Password**: `Admin123!`
- **Client Test Account**:
  - **Email**: `client@nexusstudio.io`
  - **Password**: `Client123!`

### 2. Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarkGraphikoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
    "SqliteConnection": "Data Source=markgraphiko.db"
  },
  "DatabaseProvider": "Sqlite", 
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  },
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY"
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587",
    "Username": "apikey",
    "Password": "",
    "EnableSsl": true
  }
}
```

*Note: To switch to SQL Server, update `"DatabaseProvider": "SqlServer"` and configure your SQL Server connection string in `"DefaultConnection"`.*

### 3. Run Locally
```bash
# Restore and build
dotnet restore
dotnet build

# Run application
dotnet run --urls "http://127.0.0.1:5000"
```
Open your browser to `http://127.0.0.1:5000`.
