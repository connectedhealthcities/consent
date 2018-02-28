﻿// <auto-generated />
using CHC.Consent.Common;
using CHC.Consent.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace CHC.Consent.EFCore.Migrations
{
    [DbContext(typeof(ConsentContext))]
    partial class ConsentContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.BradfordHospitalNumberEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("HospitalNumber");

                    b.Property<long?>("PersonId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("BradfordHosptialNumber");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.IdentifierEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<DateTime?>("Deleted");

                    b.Property<long?>("PersonId")
                        .IsRequired();

                    b.Property<string>("TypeName");

                    b.Property<string>("Value");

                    b.Property<string>("ValueType");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("IdentifierEntity");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.MedwayNameEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<long?>("PersonId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("MedwayNameEntity");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.NameEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EffectiveFrom");

                    b.Property<DateTime?>("EffectiveTo");

                    b.Property<string>("Family");

                    b.Property<string>("Given");

                    b.Property<long?>("PersonId")
                        .IsRequired();

                    b.Property<string>("Prefix");

                    b.Property<string>("Suffix");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("PersonName");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.PersonEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BirthOrderValue")
                        .HasColumnName("BirthOrder");

                    b.Property<DateTime?>("DateOfBirth");

                    b.Property<string>("NhsNumber");

                    b.Property<int?>("Sex");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.BradfordHospitalNumberEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.IdentifierEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.MedwayNameEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("CHC.Consent.EFCore.Entities.NameEntity", b =>
                {
                    b.HasOne("CHC.Consent.EFCore.Entities.PersonEntity", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
