using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Data
{
    public class SafeNodeDbContext(DbContextOptions<SafeNodeDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileRecord>().HasQueryFilter(f => !f.IsDeleted);

            modelBuilder.Entity<FileRecord>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserMaster>()
                .Property(u => u.Role)
                .HasConversion<string>() 
                .HasColumnType("varchar(20)")
                .HasMaxLength(20);
        }

        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<FileRecord> FileRecords { get; set; }
        public DbSet<Folder> Folders { get; set; }
    }
}
