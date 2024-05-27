using Microsoft.EntityFrameworkCore;

namespace ImageService.Models
{
    public class ImageServiceContext : DbContext
    {
        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<Image> Images { get; set; }

        public virtual DbSet<ImageCategory> ImageCategories { get; set; }

        public virtual DbSet<Like> Likes { get; set; }

        public virtual DbSet<SavedImage> SavedImages { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public ImageServiceContext(DbContextOptions<ImageServiceContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many relationship between User and Image
            modelBuilder.Entity<Image>()
                .HasOne(i => i.User)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the many-to-many relationship between Image and Category
            modelBuilder.Entity<ImageCategory>()
                .HasKey(ic => ic.Id);

            modelBuilder.Entity<ImageCategory>()
                .HasOne(ic => ic.Image)
                .WithMany(i => i.ImageCategories)
                .HasForeignKey(ic => ic.ImageId);

            modelBuilder.Entity<ImageCategory>()
                .HasOne(ic => ic.Category)
                .WithMany(c => c.ImageCategories)
                .HasForeignKey(ic => ic.CategoryId);

            // Configure the one-to-many relationships for Like and SavedImage
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Image)
                .WithMany(i => i.Likes)
                .HasForeignKey(l => l.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedImage>()
                .HasOne(si => si.Image)
                .WithMany(i => i.SavedImages)
                .HasForeignKey(si => si.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SavedImage>()
                .HasOne(si => si.User)
                .WithMany(u => u.SavedImages)
                .HasForeignKey(si => si.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
