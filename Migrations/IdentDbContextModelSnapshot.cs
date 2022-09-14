﻿// <auto-generated />
using System;
using MatrixIdent.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MatrixIdent.Migrations
{
    [DbContext(typeof(IdentDbContext))]
    partial class IdentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("MatrixIdent.Models.AuthItem", b =>
                {
                    b.Property<string>("token")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("access_token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("expires_in")
                        .HasColumnType("bigint");

                    b.Property<string>("matrix_server_name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("token_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("user_id")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("token");

                    b.ToTable("AuthItems");
                });

            modelBuilder.Entity("MatrixIdent.Models.EmailValidationRequestItem", b =>
                {
                    b.Property<string>("email")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("client_secret")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("expire_after")
                        .HasColumnType("bigint");

                    b.Property<string>("mxid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("next_link")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("send_attempt")
                        .HasColumnType("int");

                    b.Property<string>("sid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("success")
                        .HasColumnType("bit");

                    b.Property<string>("token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("email");

                    b.ToTable("EmailValidationItems");
                });

            modelBuilder.Entity("MatrixIdent.Models.HashItem", b =>
                {
                    b.Property<string>("token")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("lookup_pepper")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("token");

                    b.ToTable("HashItems");
                });

            modelBuilder.Entity("MatrixIdent.Models.InvitationRequestItem", b =>
                {
                    b.Property<string>("address")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("medium")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_alias")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_avatar_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_id")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_join_rules")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("room_type")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sender_avatar_url")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("sender_display_name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("address");

                    b.ToTable("InvitationRequestItems");
                });

            modelBuilder.Entity("MatrixIdent.Models.Key", b =>
                {
                    b.Property<string>("identifier")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long>("expiration_timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("private_key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("public_key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("identifier");

                    b.ToTable("Keys");
                });

            modelBuilder.Entity("MatrixIdent.Models.MsisdnValidationRequestItem", b =>
                {
                    b.Property<string>("phone_number")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("client_secret")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("expire_after")
                        .HasColumnType("bigint");

                    b.Property<string>("mxid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("next_link")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("send_attempt")
                        .HasColumnType("int");

                    b.Property<string>("sid")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("success")
                        .HasColumnType("bit");

                    b.Property<string>("token")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("phone_number");

                    b.ToTable("MsisdnValidationItems");
                });

            modelBuilder.Entity("MatrixIdent.Models.ThreePidResponseItem", b =>
                {
                    b.Property<string>("address")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("medium")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("mxid")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("not_after")
                        .HasColumnType("bigint");

                    b.Property<long>("not_before")
                        .HasColumnType("bigint");

                    b.Property<long>("ts")
                        .HasColumnType("bigint");

                    b.HasKey("address");

                    b.ToTable("ThreePidResponseItems");
                });
#pragma warning restore 612, 618
        }
    }
}
