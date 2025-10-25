using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRN_MANGA_PROJECT.Models.Entities;

namespace PRN_MANGA_PROJECT.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Manga> Mangas { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ChapterImage> ChapterImages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<MangaTag> MangaTags { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<ReadingHistory> ReadingHistories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure MangaTag as a many-to-many relationship
            builder.Entity<MangaTag>()
                .HasKey(mt => new { mt.MangaId, mt.TagId });

            builder.Entity<MangaTag>()
                .HasOne(mt => mt.Manga)
                .WithMany(m => m.MangaTags)
                .HasForeignKey(mt => mt.MangaId);

            builder.Entity<MangaTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MangaTags)
                .HasForeignKey(mt => mt.TagId);

            // Configure Comment self-referencing relationship
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); 

            builder.Entity<Comment>()
                .HasOne(c => c.Chapter)
                .WithMany(ch => ch.Comments)
                .HasForeignKey(c => c.ChapterId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Configure indexes for better performance
            builder.Entity<Manga>()
                .HasIndex(m => m.Title);

            builder.Entity<Manga>()
                .HasIndex(m => m.Status);

            builder.Entity<Chapter>()
                .HasIndex(c => c.MangaId);

            builder.Entity<Chapter>()
                .HasIndex(c => c.ChapterNumber);

            builder.Entity<Bookmark>()
                .HasIndex(b => b.UserId);

            builder.Entity<Bookmark>()
                .HasIndex(b => new { b.UserId, b.MangaId })
                .IsUnique();

            builder.Entity<ReadingHistory>()
                .HasIndex(rh => rh.UserId);

            builder.Entity<ReadingHistory>()
                .HasIndex(rh => rh.MangaId);

            builder.Entity<Comment>()
                .HasIndex(c => c.ChapterId);

            builder.Entity<Comment>()
                .HasIndex(c => c.UserId);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
            }
        }
    }
}
