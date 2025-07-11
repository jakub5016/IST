﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PatientService.Infrastracture.Database;

#nullable disable

namespace PatientService.Migrations
{
    [DbContext(typeof(PatientContext))]
    partial class PatientContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("patient")
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PatientService.Domain.Patient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("first_name");

                    b.Property<bool>("IsIdentityConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_identity_confirmed");

                    b.Property<bool>("IsPhoneConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_phone_confirmed");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("last_name");

                    b.Property<string>("PESEL")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pesel");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.HasKey("Id")
                        .HasName("pk_patients");

                    b.HasIndex("PESEL")
                        .IsUnique()
                        .HasDatabaseName("ix_patients_pesel");

                    b.ToTable("patients", "patient");
                });
#pragma warning restore 612, 618
        }
    }
}
