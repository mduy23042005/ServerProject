using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Server.Models;

public partial class ServerDbprojectContext : DbContext
{
    public ServerDbprojectContext()
    {
    }

    public ServerDbprojectContext(DbContextOptions<ServerDbprojectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMessage> GroupMessages { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-AC2MH2TQ\\THELMOD;Database=ServerDBProject;User Id=sa;Password=mduy23042005;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Idaccount).HasName("PK__Account__1D323F906F32C2C5");

            entity.ToTable("Account");

            entity.Property(e => e.Idaccount).HasColumnName("IDAccount");
            entity.Property(e => e.State).HasMaxLength(255);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Idgroup).HasName("PK__Group__CB4260CA241A5665");

            entity.ToTable("Group");

            entity.Property(e => e.Idgroup).HasColumnName("IDGroup");
            entity.Property(e => e.Idaccount).HasColumnName("IDAccount");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.Idaccount)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Group_Account");
        });

        modelBuilder.Entity<GroupMessage>(entity =>
        {
            entity.HasKey(e => e.IdgroupMessage).HasName("PK__GroupMes__C4D02006570C6C55");

            entity.Property(e => e.IdgroupMessage).HasColumnName("IDGroupMessage");
            entity.Property(e => e.Contents).HasColumnType("text");
            entity.Property(e => e.IdaccountFrom).HasColumnName("IDAccountFrom");
            entity.Property(e => e.Idgroup).HasColumnName("IDGroup");

            entity.HasOne(d => d.IdaccountFromNavigation).WithMany(p => p.GroupMessages)
                .HasForeignKey(d => d.IdaccountFrom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GroupMessages_Account");

            entity.HasOne(d => d.IdgroupNavigation).WithMany(p => p.GroupMessages)
                .HasForeignKey(d => d.Idgroup)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GroupMessages_Group");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Idmessage).HasName("PK__Messages__195595EC21B9CB8F");

            entity.Property(e => e.Idmessage).HasColumnName("IDMessage");
            entity.Property(e => e.Contents).HasColumnType("text");
            entity.Property(e => e.IdaccountFrom).HasColumnName("IDAccountFrom");
            entity.Property(e => e.IdaccountTo).HasColumnName("IDAccountTo");

            entity.HasOne(d => d.IdaccountFromNavigation).WithMany(p => p.MessageIdaccountFromNavigations)
                .HasForeignKey(d => d.IdaccountFrom)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_AccountFrom");

            entity.HasOne(d => d.IdaccountToNavigation).WithMany(p => p.MessageIdaccountToNavigations)
                .HasForeignKey(d => d.IdaccountTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Message_AccountTo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
