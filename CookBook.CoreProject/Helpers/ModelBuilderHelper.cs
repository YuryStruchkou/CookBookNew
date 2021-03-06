﻿using System;
using CookBook.Domain.Enums;
using CookBook.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CookBook.CoreProject.Helpers
{
    public static class ModelBuilderHelper
    {
        public static void SetupPrimaryKeys(this ModelBuilder builder)
        {
            builder.Entity<RefreshToken>()
                .HasKey(t => t.Token);

            builder.Entity<UserProfile>()
                .HasKey(u => u.UserId);

            builder.Entity<Recipe>()
                .HasKey(r => r.Id);
            builder.Entity<Recipe>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Comment>()
                .HasKey(c => c.Id);
            builder.Entity<Comment>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Tag>()
                .HasKey(t => t.Id);
            builder.Entity<Tag>()
                .Property(t => t.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Vote>()
                .HasKey(v => v.Id);
            builder.Entity<Vote>()
                .Property(v => v.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<RecipeTag>()
                .HasKey(rt => new { rt.RecipeId, rt.TagId });
        }

        public static void SetupAlternateKeys(this ModelBuilder builder)
        {
            builder.Entity<Tag>()
                .HasAlternateKey(t => t.Content);
            builder.Entity<Vote>()
                .HasAlternateKey(v => new {v.RecipeId, v.UserId});
        }

        public static void SetupRequiredColumns(this ModelBuilder builder)
        {
            builder.Entity<RefreshToken>()
                .Property(t => t.ExpiryDate)
                .IsRequired();
            builder.Entity<RefreshToken>()
                .Property(t => t.UserId)
                .IsRequired();

            builder.Entity<UserProfile>()
                .Property(u => u.IsMuted)
                .IsRequired();
            builder.Entity<UserProfile>()
                .Property(u => u.UserStatus)
                .IsRequired();

            builder.Entity<Recipe>()
                .Property(r => r.Name)
                .IsRequired();
            builder.Entity<Recipe>()
                .Property(r => r.Description)
                .IsRequired();
            builder.Entity<Recipe>()
                .Property(r => r.Content)
                .IsRequired();
            builder.Entity<Recipe>()
                .Property(r => r.CreationDate)
                .IsRequired();
            builder.Entity<Recipe>()
                .Property(r => r.RecipeStatus)
                .IsRequired();

            builder.Entity<Comment>()
                .Property(c => c.Content)
                .IsRequired();
            builder.Entity<Comment>()
                .Property(c => c.CreationDate)
                .IsRequired();
            builder.Entity<Comment>()
                .Property(c => c.RecipeId)
                .IsRequired();

            builder.Entity<Vote>()
                .Property(v => v.Value)
                .IsRequired();
        }

        public static void SetupRelations(this ModelBuilder builder)
        {
            builder.Entity<RefreshToken>()
                .HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserProfile>()
                .HasOne(up => up.ApplicationUser)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Comment>()
                .HasOne(c => c.Recipe)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Vote>()
                .HasOne(v => v.Recipe)
                .WithMany(r => r.Votes)
                .HasForeignKey(v => v.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecipeTag>()
                .HasOne(rt => rt.Recipe)
                .WithMany(r => r.RecipeTags)
                .HasForeignKey(rt => rt.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecipeTag>()
                .HasOne(rt => rt.Tag)
                .WithMany(t => t.RecipeTags)
                .HasForeignKey(rt => rt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public static void SetupEnumConversions(this ModelBuilder builder)
        {
            builder.Entity<UserProfile>()
                .Property(u => u.UserStatus)
                .HasConversion(s => s.ToString(), s => Enum.Parse<UserStatus>(s));
            builder.Entity<Recipe>()
                .Property(u => u.RecipeStatus)
                .HasConversion(s => s.ToString(), s => Enum.Parse<RecipeStatus>(s));
        }

        public static void SetupIndices(this ModelBuilder builder)
        {
            builder.Entity<RefreshToken>()
                .HasIndex(u => u.ExpiryDate);
        }
    }
}
