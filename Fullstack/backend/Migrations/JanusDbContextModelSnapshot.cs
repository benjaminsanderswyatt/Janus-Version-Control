﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using backend.Models;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(JanusDbContext))]
    partial class JanusDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_unicode_ci")
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("backend.Models.AccessTokenBlacklist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("BlacklistedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("AccessTokenBlacklists");
                });

            modelBuilder.Entity("backend.Models.Branch", b =>
                {
                    b.Property<int>("BranchId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BranchId"));

                    b.Property<string>("BranchName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreatedBy")
                        .HasColumnType("int");

                    b.Property<int?>("ParentBranch")
                        .HasColumnType("int");

                    b.Property<int>("RepoId")
                        .HasColumnType("int");

                    b.HasKey("BranchId");

                    b.HasIndex("ParentBranch");

                    b.HasIndex("RepoId", "BranchName")
                        .IsUnique();

                    b.ToTable("Branches");

                    b.HasData(
                        new
                        {
                            BranchId = 1,
                            BranchName = "main",
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(8943),
                            CreatedBy = 1,
                            RepoId = 1
                        },
                        new
                        {
                            BranchId = 2,
                            BranchName = "branch",
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(8947),
                            CreatedBy = 1,
                            ParentBranch = 1,
                            RepoId = 1
                        });
                });

            modelBuilder.Entity("backend.Models.Commit", b =>
                {
                    b.Property<int>("CommitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("CommitId"));

                    b.Property<string>("AuthorEmail")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AuthorName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<int>("BranchId")
                        .HasColumnType("int");

                    b.Property<string>("CommitHash")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(40)");

                    b.Property<DateTime>("CommittedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("TreeHash")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("varchar(40)");

                    b.HasKey("CommitId");

                    b.HasIndex("BranchId");

                    b.HasIndex("CommitHash");

                    b.HasIndex("CommittedAt");

                    b.ToTable("Commits");

                    b.HasData(
                        new
                        {
                            CommitId = 1,
                            AuthorEmail = "janus",
                            AuthorName = "janus",
                            BranchId = 1,
                            CommitHash = "f7b1c205158daf2ee72d31cc1838455368c15cb3",
                            CommittedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(9004),
                            Message = "Initial commit",
                            TreeHash = ""
                        },
                        new
                        {
                            CommitId = 2,
                            AuthorEmail = "user@2.com",
                            AuthorName = "User2",
                            BranchId = 2,
                            CommitHash = "915b84e9f8ce43018350092a25c4f65e6e290165",
                            CommittedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(9006),
                            Message = "Next commit",
                            TreeHash = "c65dca236a008513a28342c778c7c34a0b9b50f0"
                        });
                });

            modelBuilder.Entity("backend.Models.CommitParent", b =>
                {
                    b.Property<int>("ChildId")
                        .HasColumnType("int");

                    b.Property<int>("ParentId")
                        .HasColumnType("int");

                    b.HasKey("ChildId", "ParentId");

                    b.HasIndex("ParentId");

                    b.ToTable("CommitParents");

                    b.HasData(
                        new
                        {
                            ChildId = 2,
                            ParentId = 1
                        });
                });

            modelBuilder.Entity("backend.Models.RepoAccess", b =>
                {
                    b.Property<int>("RepoId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("AccessLevel")
                        .HasColumnType("int");

                    b.HasKey("RepoId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("RepoAccess");

                    b.HasData(
                        new
                        {
                            RepoId = 1,
                            UserId = 1,
                            AccessLevel = 3
                        },
                        new
                        {
                            RepoId = 1,
                            UserId = 2,
                            AccessLevel = 1
                        },
                        new
                        {
                            RepoId = 2,
                            UserId = 2,
                            AccessLevel = 3
                        });
                });

            modelBuilder.Entity("backend.Models.Repository", b =>
                {
                    b.Property<int>("RepoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("RepoId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("RepoDescription")
                        .IsRequired()
                        .HasMaxLength(511)
                        .HasColumnType("varchar(511)");

                    b.Property<string>("RepoName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("RepoId");

                    b.HasIndex("OwnerId", "RepoName")
                        .IsUnique();

                    b.ToTable("Repositories");

                    b.HasData(
                        new
                        {
                            RepoId = 1,
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(8631),
                            IsPrivate = false,
                            OwnerId = 1,
                            RepoDescription = "First seeded",
                            RepoName = "Repo1"
                        },
                        new
                        {
                            RepoId = 2,
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(8635),
                            IsPrivate = true,
                            OwnerId = 2,
                            RepoDescription = "Sec seeded",
                            RepoName = "Repo2"
                        });
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("UserId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ProfilePicturePath")
                        .HasColumnType("longtext");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("RefreshTokenExpiryTime")
                        .HasColumnType("datetime(6)");

                    b.Property<byte[]>("Salt")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(63)
                        .HasColumnType("varchar(63)");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(7979),
                            Email = "user@1.com",
                            PasswordHash = "mEWh1NibjVpyw3wCpiRbSn6ECHhd/ilre+g5KgGo0yo=",
                            Salt = new byte[] { 36, 235, 44, 217, 238, 128, 97, 108, 91, 206, 49, 83, 138, 106, 19, 217 },
                            Username = "User1"
                        },
                        new
                        {
                            UserId = 2,
                            CreatedAt = new DateTime(2025, 2, 17, 21, 18, 47, 725, DateTimeKind.Utc).AddTicks(7997),
                            Email = "user@2.com",
                            PasswordHash = "Bllp1xqdsOG8umjdxAk1UhQYqiDPF6po/3s2sTQGeN4=",
                            Salt = new byte[] { 50, 111, 0, 136, 139, 5, 123, 10, 34, 89, 80, 143, 5, 99, 163, 201 },
                            Username = "User2"
                        });
                });

            modelBuilder.Entity("backend.Models.Branch", b =>
                {
                    b.HasOne("backend.Models.Branch", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentBranch");

                    b.HasOne("backend.Models.Repository", "Repository")
                        .WithMany("Branches")
                        .HasForeignKey("RepoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Parent");

                    b.Navigation("Repository");
                });

            modelBuilder.Entity("backend.Models.Commit", b =>
                {
                    b.HasOne("backend.Models.Branch", "Branch")
                        .WithMany("Commits")
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");
                });

            modelBuilder.Entity("backend.Models.CommitParent", b =>
                {
                    b.HasOne("backend.Models.Commit", "Child")
                        .WithMany("Parents")
                        .HasForeignKey("ChildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend.Models.Commit", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Child");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("backend.Models.RepoAccess", b =>
                {
                    b.HasOne("backend.Models.Repository", "Repository")
                        .WithMany("RepoAccesses")
                        .HasForeignKey("RepoId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("backend.Models.User", "User")
                        .WithMany("RepoAccesses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Repository");

                    b.Navigation("User");
                });

            modelBuilder.Entity("backend.Models.Repository", b =>
                {
                    b.HasOne("backend.Models.User", "Owner")
                        .WithMany("Repositories")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("backend.Models.Branch", b =>
                {
                    b.Navigation("Commits");
                });

            modelBuilder.Entity("backend.Models.Commit", b =>
                {
                    b.Navigation("Children");

                    b.Navigation("Parents");
                });

            modelBuilder.Entity("backend.Models.Repository", b =>
                {
                    b.Navigation("Branches");

                    b.Navigation("RepoAccesses");
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Navigation("RepoAccesses");

                    b.Navigation("Repositories");
                });
#pragma warning restore 612, 618
        }
    }
}
