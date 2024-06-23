﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Roadmap.Infrastructure;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240623074325_topicDeleted")]
    partial class topicDeleted
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Roadmap.Domain.Entities.ExpiredToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TokenString")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TokenString")
                        .IsUnique();

                    b.ToTable("ExpiredTokens");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.PrivateAccess", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoadmapId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoadmapId");

                    b.HasIndex("UserId");

                    b.ToTable("PrivateAccesses");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.RefreshToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TokenString")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TokenString")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.Roadmap", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<JsonDocument>("Content")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Roadmaps");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.StaredRoadmap", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoadmapId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoadmapId");

                    b.HasIndex("UserId");

                    b.ToTable("StaredRoadmaps");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<List<Guid>>("RecentlyVisited")
                        .HasColumnType("uuid[]");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.PrivateAccess", b =>
                {
                    b.HasOne("Roadmap.Domain.Entities.Roadmap", "Roadmap")
                        .WithMany("PrivateAccesses")
                        .HasForeignKey("RoadmapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Roadmap.Domain.Entities.User", "User")
                        .WithMany("PrivateAccesses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Roadmap");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.RefreshToken", b =>
                {
                    b.HasOne("Roadmap.Domain.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.Roadmap", b =>
                {
                    b.HasOne("Roadmap.Domain.Entities.User", "User")
                        .WithMany("CreatedRoadmaps")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.StaredRoadmap", b =>
                {
                    b.HasOne("Roadmap.Domain.Entities.Roadmap", "Roadmap")
                        .WithMany("Stared")
                        .HasForeignKey("RoadmapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Roadmap.Domain.Entities.User", "User")
                        .WithMany("StaredRoadmaps")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Roadmap");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.Roadmap", b =>
                {
                    b.Navigation("PrivateAccesses");

                    b.Navigation("Stared");
                });

            modelBuilder.Entity("Roadmap.Domain.Entities.User", b =>
                {
                    b.Navigation("CreatedRoadmaps");

                    b.Navigation("PrivateAccesses");

                    b.Navigation("RefreshTokens");

                    b.Navigation("StaredRoadmaps");
                });
#pragma warning restore 612, 618
        }
    }
}
