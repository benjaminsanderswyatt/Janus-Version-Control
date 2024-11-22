﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using backend.Models;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(JanusDbContext))]
    [Migration("20241119171156_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("backend.Models.Branch", b =>
                {
                    b.Property<int>("BranchId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BranchId"));

                    b.Property<string>("BranchName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("LatestCommitId")
                        .HasColumnType("int");

                    b.Property<int>("RepoId")
                        .HasColumnType("int");

                    b.HasKey("BranchId");

                    b.HasIndex("RepoId");

                    b.ToTable("Branches");
                });

            modelBuilder.Entity("backend.Models.Collaborator", b =>
                {
                    b.Property<int>("RepoId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("RepoId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Collaborators");
                });

            modelBuilder.Entity("backend.Models.Commit", b =>
                {
                    b.Property<int>("CommitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("CommitId"));

                    b.Property<int>("BranchId")
                        .HasColumnType("int");

                    b.Property<string>("CommitHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CommittedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<int?>("ParentCommitId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CommitId");

                    b.HasIndex("BranchId");

                    b.HasIndex("UserId");

                    b.ToTable("Commits");
                });

            modelBuilder.Entity("backend.Models.File", b =>
                {
                    b.Property<int>("FileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("FileId"));

                    b.Property<int>("CommitId")
                        .HasColumnType("int");

                    b.Property<string>("FileHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("FileId");

                    b.HasIndex("CommitId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("backend.Models.FileContent", b =>
                {
                    b.Property<int>("ContentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ContentId"));

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<int>("FileId")
                        .HasColumnType("int");

                    b.HasKey("ContentId");

                    b.HasIndex("FileId");

                    b.ToTable("FileContents");
                });

            modelBuilder.Entity("backend.Models.Repository", b =>
                {
                    b.Property<int>("RepoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("RepoId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("RepoName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("Visibility")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("RepoId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Repositories");
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
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("backend.Models.Branch", b =>
                {
                    b.HasOne("backend.Models.Repository", "Repository")
                        .WithMany("Branches")
                        .HasForeignKey("RepoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Repository");
                });

            modelBuilder.Entity("backend.Models.Collaborator", b =>
                {
                    b.HasOne("backend.Models.Repository", "Repository")
                        .WithMany("Collaborators")
                        .HasForeignKey("RepoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend.Models.User", "User")
                        .WithMany("Collaborators")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Repository");

                    b.Navigation("User");
                });

            modelBuilder.Entity("backend.Models.Commit", b =>
                {
                    b.HasOne("backend.Models.Branch", "Branch")
                        .WithMany("Commits")
                        .HasForeignKey("BranchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Branch");

                    b.Navigation("User");
                });

            modelBuilder.Entity("backend.Models.File", b =>
                {
                    b.HasOne("backend.Models.Commit", "Commit")
                        .WithMany("Files")
                        .HasForeignKey("CommitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Commit");
                });

            modelBuilder.Entity("backend.Models.FileContent", b =>
                {
                    b.HasOne("backend.Models.File", "File")
                        .WithMany("FileContents")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("File");
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
                    b.Navigation("Files");
                });

            modelBuilder.Entity("backend.Models.File", b =>
                {
                    b.Navigation("FileContents");
                });

            modelBuilder.Entity("backend.Models.Repository", b =>
                {
                    b.Navigation("Branches");

                    b.Navigation("Collaborators");
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Navigation("Collaborators");

                    b.Navigation("Repositories");
                });
#pragma warning restore 612, 618
        }
    }
}
