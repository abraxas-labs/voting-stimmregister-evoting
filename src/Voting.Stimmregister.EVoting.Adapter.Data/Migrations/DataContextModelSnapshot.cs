﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Voting.Stimmregister.EVoting.Adapter.Data;

#nullable disable

namespace Voting.Stimmregister.EVoting.Adapter.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.DocumentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<byte[]>("Document")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("StatusChangeId")
                        .HasColumnType("uuid");

                    b.Property<string>("WorkerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("StatusChangeId")
                        .IsUnique();

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.EVotingStatusChangeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<string>("ContextId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("EVotingRegistered")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("ContextId")
                        .IsUnique();

                    b.ToTable("EVotingStatusChanges");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.PersonEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("Ahvn13")
                        .HasColumnType("bigint");

                    b.Property<bool>("AllowedToVote")
                        .HasColumnType("boolean");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<short>("MunicipalityBfs")
                        .HasColumnType("smallint");

                    b.Property<string>("Nationality")
                        .HasColumnType("text");

                    b.Property<string>("OfficialName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Sex")
                        .HasColumnType("integer");

                    b.Property<Guid>("StatusChangeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("StatusChangeId")
                        .IsUnique();

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.RateLimitEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ActionCount")
                        .HasColumnType("integer");

                    b.Property<long>("Ahvn13")
                        .HasColumnType("bigint");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.HasKey("Id");

                    b.HasIndex("Ahvn13", "Date")
                        .IsUnique();

                    b.ToTable("RateLimits");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.DocumentEntity", b =>
                {
                    b.HasOne("Voting.Stimmregister.EVoting.Domain.Models.EVotingStatusChangeEntity", "StatusChange")
                        .WithOne("Document")
                        .HasForeignKey("Voting.Stimmregister.EVoting.Domain.Models.DocumentEntity", "StatusChangeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StatusChange");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.PersonEntity", b =>
                {
                    b.HasOne("Voting.Stimmregister.EVoting.Domain.Models.EVotingStatusChangeEntity", "StatusChange")
                        .WithOne("Person")
                        .HasForeignKey("Voting.Stimmregister.EVoting.Domain.Models.PersonEntity", "StatusChangeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Voting.Stimmregister.EVoting.Domain.Models.AddressEntity", "Address", b1 =>
                        {
                            b1.Property<Guid>("PersonEntityId")
                                .HasColumnType("uuid");

                            b1.Property<string>("HouseNumber")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("PostOfficeBoxText")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Town")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("PersonEntityId");

                            b1.ToTable("Persons");

                            b1.WithOwner()
                                .HasForeignKey("PersonEntityId");
                        });

                    b.Navigation("Address")
                        .IsRequired();

                    b.Navigation("StatusChange");
                });

            modelBuilder.Entity("Voting.Stimmregister.EVoting.Domain.Models.EVotingStatusChangeEntity", b =>
                {
                    b.Navigation("Document");

                    b.Navigation("Person");
                });
#pragma warning restore 612, 618
        }
    }
}
