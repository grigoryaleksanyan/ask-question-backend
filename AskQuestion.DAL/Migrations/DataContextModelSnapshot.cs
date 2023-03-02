﻿// <auto-generated />
using System;
using AskQuestion.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AskQuestion.DAL.Entities.FaqCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("Updated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("Сreated")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("FaqCategories");
                });

            modelBuilder.Entity("AskQuestion.DAL.Entities.FaqEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("FaqCategoryId")
                        .HasColumnType("uuid");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("Updated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("Сreated")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("FaqCategoryId");

                    b.ToTable("FaqEntries");
                });

            modelBuilder.Entity("AskQuestion.DAL.Entities.Question", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("Answered")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Author")
                        .HasColumnType("text");

                    b.Property<int>("Dislikes")
                        .HasColumnType("integer");

                    b.Property<int>("Likes")
                        .HasColumnType("integer");

                    b.Property<string>("Speaker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Views")
                        .HasColumnType("integer");

                    b.Property<string>("Zone")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Сreated")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("AskQuestion.DAL.Entities.FaqEntry", b =>
                {
                    b.HasOne("AskQuestion.DAL.Entities.FaqCategory", "FaqCategory")
                        .WithMany("FaqEntries")
                        .HasForeignKey("FaqCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FaqCategory");
                });

            modelBuilder.Entity("AskQuestion.DAL.Entities.FaqCategory", b =>
                {
                    b.Navigation("FaqEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
