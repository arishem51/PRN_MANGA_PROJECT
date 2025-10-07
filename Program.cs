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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
// Add GG
builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// Add API Controllers
builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
});

builder.Services.AddRazorPages(options =>
{
    // Secure the entire Admin area to Admin role only
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
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
