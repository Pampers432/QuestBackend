using System;
using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QuestsApi.Data;

public partial class QuestPlatformContext : DbContext
{
    public QuestPlatformContext(DbContextOptions<QuestPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnswerOption> AnswerOptions { get; set; }

    public virtual DbSet<Attempt> Attempts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Quest> Quests { get; set; }

    public virtual DbSet<QuestRoom> QuestRooms { get; set; }

    public virtual DbSet<RegistrationToken> RegistrationTokens { get; set; }

    public virtual DbSet<QuestSession> QuestSessions { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RoomTemplate> RoomTemplates { get; set; }

    public virtual DbSet<TemplateRename> TemplateRenames { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Attachment).HasMaxLength(255);

            entity.HasOne(d => d.Question).WithMany(p => p.AnswerOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_AnswerOptions_Questions");
        });

        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.QuestSession).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.QuestSessionId)
                .HasConstraintName("FK_Attempts_Sessions");

            entity.HasOne(d => d.User).WithMany(p => p.Attempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attempts_Users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Quest>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Difficulty).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.Quests)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quests_Users");

            entity.HasOne(d => d.Category).WithMany(p => p.Quests)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Quests_Categories");
        });

        modelBuilder.Entity<QuestRoom>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Quest).WithMany(p => p.QuestRooms)
                .HasForeignKey(d => d.QuestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_QuestRooms_Quests");

            entity.HasOne(d => d.RoomTemplate).WithMany(p => p.QuestRooms)
                .HasForeignKey(d => d.RoomTemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestRooms_Templates");
        });

        modelBuilder.Entity<RegistrationToken>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasIndex(e => e.Email, "UQ__Registra__9A2B249A5C9F217E").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RegistrationTokens_Users");
        });

        modelBuilder.Entity<QuestSession>(entity =>
        {
            entity.HasIndex(e => e.AccessCode, "UQ__QuestSes__24C20D0C80A32EF2").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccessCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartsAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Quest).WithMany(p => p.QuestSessions)
                .HasForeignKey(d => d.QuestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_QuestSessions_Quests");

            entity.HasOne(d => d.StartedByNavigation).WithMany(p => p.QuestSessions)
                .HasForeignKey(d => d.StartedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestSessions_Users");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Attachment).HasMaxLength(255);
            entity.Property(e => e.TargetObject).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.QuestRoom).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestRoomId)
                .HasConstraintName("FK_Questions_QuestRooms");
        });

        modelBuilder.Entity<RoomTemplate>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PreviewImage).HasMaxLength(255);
        });

        modelBuilder.Entity<TemplateRename>(entity =>
        {
            entity.Property(e => e.SystemKey).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(100);

            entity.HasOne(d => d.Template).WithMany(p => p.TemplateRenames)
                .HasForeignKey(d => d.TemplateId)
                .HasConstraintName("FK_TemplateRenames_RoomTemplates")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany(p => p.TemplateRenames)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TemplateRenames_Users")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ__Users__536C85E454869B91").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AnsweredAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Attempt).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.AttemptId)
                .HasConstraintName("FK_UserAnswers_Attempts");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswers_Questions");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
