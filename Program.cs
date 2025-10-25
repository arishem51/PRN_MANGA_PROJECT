using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Repositories.Auth;
using PRN_MANGA_PROJECT.Services;
using PRN_MANGA_PROJECT.Services.Auth;
using PRN_MANGA_PROJECT.Services.EmailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using PRN_MANGA_PROJECT.Repositories.CRUD;
using PRN_MANGA_PROJECT.Services.CRUD;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Entity Framework
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

// Add MudBlazor
builder.Services.AddMudServices();

// Add Repositories
builder.Services.AddScoped<IBaseRepository<Manga>, BaseRepository<Manga>>();
builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<IBaseRepository<Chapter>, BaseRepository<Chapter>>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IBaseRepository<Tag>, BaseRepository<Tag>>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IBaseRepository<Bookmark>, BaseRepository<Bookmark>>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
// Add Services
builder.Services.AddScoped<IMangaService, MangaService>();
builder.Services.AddScoped<IChapterService, ChapterService>();

// Add HttpClient for ChapterUIService
builder.Services.AddHttpClient<ChapterUIService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5274/");
});

// Add ChapterUIService
builder.Services.AddScoped<ChapterUIService>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

//Auth Logic
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService , UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
// Add Google Authentication (only if credentials are provided)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret) && 
    googleClientId != "your-google-client-id-here" && googleClientSecret != "your-google-client-secret-here")
{
    builder.Services.AddAuthentication()
        .AddGoogle("Google", options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}
else
{
    // Add authentication without Google provider if credentials are not configured
    builder.Services.AddAuthentication();
}

// Add API Controllers
builder.Services.AddControllers();
builder.Services.AddHttpClient("MangaDexClient", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MangaProject/1.0)");
    client.BaseAddress = new Uri("https://api.mangadex.org/");
});

//Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
    options.AddPolicy("UserOnly", policy =>
    {
        policy.RequireRole("Reader", "Admin");
    });
});

builder.Services.AddRazorPages(options =>
{
    // Secure the entire Admin area to Admin role only
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage", "UserOnly");

});

// Redirect unauthenticated and unauthorized users to home page
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

//Create Default Role
using (var scope = app.Services.CreateScope())
{
    var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
    await roleService.SeedRole();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();


// Map API Controllers
app.MapControllers();
app.MapRazorPages();

app.Run();
