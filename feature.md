# PRN Manga Project — Feature List

## 1. Authentication & Authorization

- **Login** — username/password with "remember me" (14-day cookie)
- **Google OAuth** — sign in with Google, auto-linked to existing email
- **Registration** — username, email, phone uniqueness checks; password policy (min 6 chars, uppercase, lowercase, digit); profile fields (first/last name, address, gender, birth date)
- **Email confirmation** — token-based email verify flow
- **Forgot / Reset password** — email-based reset token flow
- **Role-based access** — two roles: `Admin` and `Reader`; seeded on startup
- **Authorization policies** — `AdminOnly` (admin area), `UserOnly` (account/manage area)

---

## 2. Public Manga Browsing

- **Homepage** — paginated manga list (10 per page), search by title/author, filter by tag
- **Manga detail page** — metadata (title, author, artist, description, status, cover), tag list, chapter list (descending), first/latest chapter shortcuts, bookmark status, reading progress indicator
- **Chapter reader** — paginated image display, next/previous chapter navigation, page count

---

## 3. MangaDex Integration

Chapters store a **MangaDex Chapter ID**. When a chapter is created or updated with a valid ID, the app automatically:

1. Calls `GET https://api.mangadex.org/chapter/{mangadexChapterId}` to check for an `externalUrl`
2. If `externalUrl` exists — stores it directly on the chapter record
3. If not — calls `GET https://api.mangadex.org/at-home/server/{mangadexChapterId}` to get the CDN base URL + image hashes, then builds and stores full image URLs as `ChapterImage` records
4. Images are refreshable on chapter edit (re-fetches from MangaDex)

This means image URLs are fetched from MangaDex CDN and persisted in the local database — no real-time proxy, just import on create/update.

---

## 4. User Features

- **Bookmarks** — save/remove manga; view bookmarked list with last-read timestamp
- **Reading history** — auto-recorded per chapter; paginated history view (2 per page); AJAX reload
- **Comments** — add comments on chapters, nested replies, edit, delete
- **Comment reactions** — like/react to comments (`CommentLike` with `ReactionType`)

---

## 5. Admin Panel (`/Admin`)

### Manga Management
- List all manga (paginated)
- Create manga (title, author, artist, description, status, cover image URL)
- Edit manga

### Chapter Management
- List chapters per manga
- Create chapter — title, chapter number, **MangaDex Chapter ID** (triggers image fetch), optional text content, page count
- Edit chapter — updating the MangaDex Chapter ID re-fetches images from MangaDex

### Tag Management
- List tags with search and pagination (5 per page)
- Create / Edit / Delete (soft delete) tags — name, description, hex color
- Real-time tag list updates via SignalR (`TagHub`)

### Account Management
- List all users (paginated, searchable, filterable by role)
- Create user account with role assignment
- Edit user info and role
- Activate / Deactivate user accounts

---

## 6. Real-time (SignalR Hubs)

| Hub | Route | Purpose |
|-----|-------|---------|
| BookmarkHub | `/bookmarkHub` | Bookmark change notifications |
| CommentHub | `/commentHub` | Comment updates |
| TagHub | `/tagHub` | Tag list reload across clients |
| AccountHub | `/accountHub` | Account change notifications |
| ProfileHub | `/profileHub` | User profile updates |
| ReadingHistoryHub | `/historyHub` | Reading history updates |
| ChangeEmailHub | `/changeEmailHub` | Email change notifications |

---

## 7. REST API (`/api/chapter`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/chapter` | All chapters |
| GET | `/api/chapter/{id}` | Chapter by ID |
| GET | `/api/chapter/{id}/with-images` | Chapter with image list |
| GET | `/api/chapter/manga/{mangaId}` | Chapters by manga |
| GET | `/api/chapter/manga/{mangaId}/paged` | Paginated chapters by manga |
| GET | `/api/chapter/{id}/next` | Next chapter |
| GET | `/api/chapter/{id}/previous` | Previous chapter |
| POST | `/api/chapter` | Create chapter |
| PUT | `/api/chapter/{id}` | Update chapter |
| DELETE | `/api/chapter/{id}` | Soft delete chapter |

---

## 8. Data Model Summary

| Entity | Key Fields |
|--------|-----------|
| `Manga` | Title, Author, Artist, Description, Status, CoverImageUrl, IsActive |
| `Chapter` | MangaId, Title, ChapterNumber, MangadexChapterId, ExternalUrl, PageCount, IsActive |
| `ChapterImage` | ChapterId, ImageUrl, PageNumber |
| `Tag` | Name, Description, Color (hex), IsActive |
| `MangaTag` | MangaId, TagId (many-to-many) |
| `User` | FirstName, LastName, Gender, Address, BirthDate, IsActive (extends IdentityUser) |
| `Bookmark` | UserId, MangaId, LastReadAt |
| `ReadingHistory` | UserId, MangaId, ChapterId, PageNumber, ReadAt |
| `Comment` | UserId, ChapterId, Content, ParentCommentId (self-ref), IsActive |
| `CommentLike` | CommentId, UserId, ReactionType |

---

## 9. Architecture

- **Pattern** — Repository → Service → Razor Page
- **ORM** — Entity Framework Core with SQL Server
- **Auth** — ASP.NET Core Identity + Google OAuth 2.0
- **Real-time** — SignalR
- **UI** — Razor Pages + MudBlazor components
- **External API** — MangaDex (`https://api.mangadex.org/`)
