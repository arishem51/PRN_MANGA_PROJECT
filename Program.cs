using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PRN_MANGA_PROJECT;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Hubs;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Repositories.Auth;
using PRN_MANGA_PROJECT.Repositories.CRUD;
using PRN_MANGA_PROJECT.Services;
using PRN_MANGA_PROJECT.Services.Auth;
using PRN_MANGA_PROJECT.Services.CRUD;
using PRN_MANGA_PROJECT.Services.EmailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== Database =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        });
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableServiceProviderCaching();
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// ===== MudBlazor + SignalR =====
builder.Services.AddMudServices();
builder.Services.AddSignalR();

// ===== Repositories =====
builder.Services.AddScoped<IBaseRepository<Manga>, BaseRepository<Manga>>();
builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<IBaseRepository<Chapter>, BaseRepository<Chapter>>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IBaseRepository<Tag>, BaseRepository<Tag>>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBaseRepository<Bookmark>, BaseRepository<Bookmark>>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();

// ===== Services =====
builder.Services.AddScoped<IMangaService, MangaService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();

// ===== Identity =====
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===== Authentication (Google optional) =====
builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// ===== HTTP Client for MangaDex API =====
builder.Services.AddHttpClient("MangaDexClient", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MangaProject/1.0)");
    client.BaseAddress = new Uri("https://api.mangadex.org/");
});

// ===== Authorization =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("Reader", "Admin"));
});

// ===== Razor Pages Auth Rules =====
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage", "UserOnly");
});

// ===== Cookie Redirect Handling =====
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.AccessDeniedPath = "/Public/AccessDenied";
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/");
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect("/Public/AccessDenied");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// ===== SEED DATABASE & ROLES =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Seed Manga, Tag, MangaTag
    SeedData.Initialize(context);

    // Seed Roles
    var roleService = services.GetRequiredService<IRoleService>();
    await roleService.SeedRole();
}

// ===== Middleware =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages();
app.MapHub<AccountHub>("/accountHub");
app.MapHub<TagHub>("/tagHub");
app.MapHub<CommentHub>("/commentHub"); // Include CommentHub if required
app.Run();
