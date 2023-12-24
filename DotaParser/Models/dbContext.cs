using System;
using System.Collections.Generic;
using DotaParser.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DotaParser.Models;

public partial class dbContext : DbContext
{
    public dbContext()
    {
    }

    public dbContext(DbContextOptions<dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hero> Heroes { get; set; }

    public virtual DbSet<MainAttribute> MainAttributes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddUserSecrets<dbContext>()
        .Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString(nameof(dbContext)));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hero>(entity =>
        {
            entity.Property(e => e.HeroId)
                .ValueGeneratedNever()
                .HasColumnName("hero_id");
            entity.Property(e => e.Armor).HasColumnName("armor");
            entity.Property(e => e.AttackType).HasColumnName("attack_type");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.Damage).HasColumnName("damage");
            entity.Property(e => e.Health).HasColumnName("health");
            entity.Property(e => e.MagicResistance).HasColumnName("magic_resistance");
            entity.Property(e => e.Mana).HasColumnName("mana");
            entity.Property(e => e.MoveSpeed).HasColumnName("move_speed");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Attribute).WithMany(p => p.Heroes)
                .HasForeignKey(d => d.AttributeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Heroes_Main Attribute");

            entity.HasMany(d => d.Roles).WithMany(p => p.Heroes)
                .UsingEntity<Dictionary<string, object>>(
                    "HeroInRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_HeroInRole_Roles"),
                    l => l.HasOne<Hero>().WithMany()
                        .HasForeignKey("HeroId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_HeroInRole_Heroes"),
                    j =>
                    {
                        j.HasKey("HeroId", "RoleId");
                        j.ToTable("HeroInRole");
                        j.IndexerProperty<Guid>("HeroId").HasColumnName("hero_id");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<MainAttribute>(entity =>
        {
            entity.HasKey(e => e.AttributeId);

            entity.ToTable("Main Attribute");

            entity.Property(e => e.AttributeId)
                .ValueGeneratedNever()
                .HasColumnName("attribute_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
