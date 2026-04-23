# PRN Manga Project

A modern full-stack web application for manga reading and management built with ASP.NET Core 9.0 and Blazor Server. This project provides a comprehensive platform for users to discover, read, and manage their favorite manga series using server-side rendering and real-time updates.

## 🚀 Features

### Core Functionality

- **Manga Discovery**: Browse and search through a vast collection of manga
- **Reading Experience**: Read manga chapters with an intuitive interface
- **User Management**: Secure user registration and authentication
- **Bookmarking System**: Save favorite manga for easy access
- **Reading History**: Track your reading progress
- **Tag System**: Organize manga by categories and genres
- **Comments**: Engage with the community through comments

### Technical Features

- **Blazor Server**: Full-stack web application with server-side rendering
- **Real-time Updates**: SignalR integration for live updates
- **Responsive Design**: MudBlazor components for modern UI
- **Entity Framework**: Code-first database approach
- **Repository Pattern**: Clean architecture with separation of concerns
- **Identity Management**: ASP.NET Core Identity for user authentication

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0 with Blazor Server
- **UI Components**: MudBlazor 8.12.0
- **Database**: SQL Server with Entity Framework Core 9.0.9
- **Authentication**: ASP.NET Core Identity
- **Real-time**: SignalR
- **Architecture**: Repository Pattern with Service Layer

## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/arishem51/PRN_MANGA_PROJECT.git
cd PRN_MANGA_PROJECT
```

### 2. Configure Database

1. Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PRN_MANGA_PROJECT;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. Run the database migrations:

```bash
dotnet ef database update
```

### 3. Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## 📁 Project Structure

```
PRN_MANGA_PROJECT/
├── Components/                 # Blazor components
│   ├── Layout/                # Layout components
│   ├── Pages/                 # Page components
│   └── Shared/                # Shared components
├── Controllers/               # MVC controllers (if any)
├── Data/                      # Database context
├── Migrations/                # Entity Framework migrations
├── Models/                    # Data models
│   ├── Entities/              # Database entities
│   └── ViewModels/            # Data transfer objects
├── Repositories/              # Data access layer
├── Services/                  # Business logic layer
├── wwwroot/                   # Static files
└── Program.cs                 # Application entry point
```

## 🗄️ Database Schema

### Core Entities

- **Manga**: Main manga information (title, author, description, status)
- **Chapter**: Individual manga chapters
- **ChapterImage**: Images within chapters
- **User**: User accounts with authentication
- **Bookmark**: User's saved manga
- **ReadingHistory**: User's reading progress
- **Tag**: Categorization system
- **Comment**: User comments on manga

### Relationships

- Manga → Chapters (One-to-Many)
- Chapter → ChapterImages (One-to-Many)
- User → Bookmarks (One-to-Many)
- User → ReadingHistory (One-to-Many)
- Manga → Tags (Many-to-Many through MangaTag)
- User → Comments (One-to-Many)

## 🎯 Application Features

### Manga Management

- **Browse Manga**: View all available manga with pagination
- **Search Functionality**: Search manga by title, author, or tags
- **Manga Details**: View detailed information about each manga
- **Category Filtering**: Filter manga by tags and status
- **Popular/Recent**: View popular and recently added manga

### User Features

- **User Registration/Login**: Secure authentication system
- **Bookmarking**: Save favorite manga for easy access
- **Reading History**: Track reading progress
- **Comments**: Leave comments on manga
- **Profile Management**: Manage user profile and preferences

## 🎨 UI Components

The application uses MudBlazor components for a modern, responsive design:

- **MudCard**: Manga display cards
- **MudTextField**: Search and input fields
- **MudButton**: Action buttons
- **MudDialog**: Modal dialogs
- **MudDataGrid**: Data tables
- **MudPagination**: Pagination controls

## 🔧 Configuration

### App Settings

Configure the application through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your connection string here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Identity Settings

User authentication is configured in `Program.cs` with the following requirements:

- Password length: Minimum 6 characters
- Password must contain: uppercase, lowercase, and digits
- Unique email addresses required
- Non-alphanumeric characters optional

## 🚀 Deployment

### Local Development

1. Ensure SQL Server is running
2. Update connection string
3. Run migrations: `dotnet ef database update`
4. Start the application: `dotnet run`

### Production Deployment

1. Configure production connection string
2. Set up SSL certificates
3. Configure logging
4. Deploy to your preferred hosting platform (Azure, AWS, etc.)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push to the branch: `git push origin feature/new-feature`
5. Submit a pull request to the `main` branch

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **arishem51** - _Initial work_ - [GitHub Profile](https://github.com/arishem51)

## 🙏 Acknowledgments

- [MudBlazor](https://mudblazor.com/) for the UI components
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) for the web framework

## 📞 Support

If you encounter any issues or have questions, please:

1. Check the [Issues](https://github.com/arishem51/PRN_MANGA_PROJECT/issues) page
2. Create a new issue with detailed information
3. Contact the maintainers

---

https://api.mangadex.org/manga/{mangaId} - get manga detail

https://api.mangadex.org/at-home/server/{chapterId} - Get chapter images

https://api.mangadex.org/chapter/{chapterId} - detail chapter

https://api.mangadex.org/chapter?manga={mangaId} - all chapter of a manga
