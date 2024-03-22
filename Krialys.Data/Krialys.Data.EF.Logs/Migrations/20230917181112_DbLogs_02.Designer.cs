﻿// <auto-generated />
using System;
using Krialys.Data.EF.Logs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Krialys.Entities.Migrations
{
    [DbContext(typeof(KrialysDbContext))]
    [Migration("20230917181112_DbLogs_02")]
    partial class DbLogs_02
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.11");

            modelBuilder.Entity("Krialys.Data.EF.Logs.TM_LOG_Logs", b =>
                {
                    b.Property<int>("log_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("log_creation_date")
                        .HasColumnType("TEXT");

                    b.Property<string>("log_exception")
                        .HasColumnType("TEXT");

                    b.Property<string>("log_message")
                        .HasColumnType("TEXT");

                    b.Property<string>("log_message_details")
                        .HasColumnType("TEXT");

                    b.Property<string>("log_type")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("log_id")
                        .HasName("log_id");

                    b.ToTable("TM_LOG_Logs");
                });
#pragma warning restore 612, 618
        }
    }
}