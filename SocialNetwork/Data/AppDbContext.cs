using Microsoft.EntityFrameworkCore;
using SocialNetwork.Models;

namespace SocialNetwork.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostUserRating> PostUserRatings { get; set; }
        public DbSet<FriendsRelation> FriendsRelations { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasMany(u => u.Posts).WithOne(p => p.User).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(p => p.Posts).HasForeignKey(p => p.UserId);

            modelBuilder.Entity<FriendsRelation>().HasOne(u => u.User1).WithMany(u => u.Friends1).HasForeignKey(u => u.User1Id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FriendsRelation>().HasOne(u => u.User2).WithMany(u => u.Friends2).HasForeignKey(u => u.User2Id).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
