using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PRN_MANGA_PROJECT.Data;
using PRN_MANGA_PROJECT.Models.Entities;
using PRN_MANGA_PROJECT.Repositories;
using PRN_MANGA_PROJECT.Repositories.Auth;
using PRN_MANGA_PROJECT.Services;
using PRN_MANGA_PROJECT.Services.Auth;
using PRN_MANGA_PROJECT.Services.EmailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Add Services
builder.Services.AddScoped<IMangaService, MangaService>();
builder.Services.AddScoped<IUserService , UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// Add API Controllers
builder.Services.AddControllers();
builder.Services.AddRazorPages();
var app = builder.Build();

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
