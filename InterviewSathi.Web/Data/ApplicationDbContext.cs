using InterviewSathi.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using InterviewSathi.Web.Models.Entities.BlogsEntity;


namespace InterviewSathi.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<LikeCount> LikeCounts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<ReviewRating> ReviewRatings { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<PlatformReview> PlatformReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);
        }
    }
}
