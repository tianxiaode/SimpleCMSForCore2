using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace SimpleCMSForCore2.Models
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser,ApplicationRole,int>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<Media> Mediae { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasOne(m => m.Parent).WithMany(m => m.SubCategories)
                .HasForeignKey(m => m.ParentId);

            modelBuilder.Entity<Content>().HasOne(m => m.Category).WithMany(m => m.Contents)
                .HasForeignKey(m => m.CategoryId);

            modelBuilder.Entity<UserProfile>().HasOne(m => m.User).WithMany(m => m.UserProfiles)
                .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<ContentTag>().HasKey(m => new
            {
                m.ContentId, m.TagId
            });

            modelBuilder.Entity<ContentTag>().HasOne(m => m.Content).WithMany(m => m.ContentTags)
                .HasForeignKey(m => m.ContentId);

            modelBuilder.Entity<ContentTag>().HasOne(m => m.Tag).WithMany(m => m.ContentTags)
                .HasForeignKey(m => m.TagId);

        }
    }
}