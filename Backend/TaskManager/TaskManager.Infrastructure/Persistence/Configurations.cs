using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).HasMaxLength(50).IsRequired();

            builder.HasData(
                new { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Trabajo" },
                new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Personal" },
                new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Estudios" });
        }
    }

    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.ToTable("Task");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).HasMaxLength(100).IsRequired();
            builder.Property(t => t.Description).HasMaxLength(500);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.UserId);
        }
    }

    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.TokenHash).HasMaxLength(64).IsRequired();
            builder.HasIndex(r => r.TokenHash).IsUnique();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
