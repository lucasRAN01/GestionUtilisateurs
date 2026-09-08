#nullable disable

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using GestionUtilisateurs.DataAccess.Data;
using GestionUtilisateurs.DataAccess.Models;

namespace GestionUtilisateurs.DataAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.AuditLog", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("ActionType")
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("Date")
                    .HasColumnType("TEXT");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("TEXT");

                b.Property<string>("Details")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("TargetUsername")
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<int?>("UserId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("ActionType");

                b.HasIndex("Date");

                b.HasIndex("UserId");

                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.Permission", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Code")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("TEXT");

                b.Property<string>("Libelle")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Code")
                    .IsUnique();

                b.ToTable("Permissions");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.Role", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("DateCreation")
                    .HasColumnType("TEXT");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("TEXT");

                b.Property<string>("Nom")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Nom")
                    .IsUnique();

                b.ToTable("Roles");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.User", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateTime>("DateCreation")
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("DerniereConnexion")
                    .HasColumnType("TEXT");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("TEXT");

                b.Property<int>("FailedLoginAttempts")
                    .HasColumnType("INTEGER");

                b.Property<DateTime?>("LockoutEnd")
                    .HasColumnType("TEXT");

                b.Property<string>("Nom")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<string>("Notes")
                    .HasMaxLength(2000)
                    .HasColumnType("TEXT");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("TEXT");

                b.Property<string>("Prenom")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.Property<int>("RoleId")
                    .HasColumnType("INTEGER");

                b.Property<int>("Status")
                    .HasColumnType("INTEGER");

                b.Property<string>("Telephone")
                    .HasMaxLength(30)
                    .HasColumnType("TEXT");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.HasIndex("RoleId");

                b.HasIndex("Username")
                    .IsUnique();

                b.ToTable("Users");
            });

            modelBuilder.Entity("RolePermission", b =>
            {
                b.Property<int>("RolesId")
                    .HasColumnType("INTEGER");

                b.Property<int>("PermissionsId")
                    .HasColumnType("INTEGER");

                b.HasKey("RolesId", "PermissionsId");

                b.HasIndex("PermissionsId");

                b.ToTable("RolePermissions");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.AuditLog", b =>
            {
                b.HasOne("GestionUtilisateurs.DataAccess.Models.User", "User")
                    .WithMany("AuditLogs")
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.Navigation("User");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.User", b =>
            {
                b.HasOne("GestionUtilisateurs.DataAccess.Models.Role", "Role")
                    .WithMany("Users")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Role");
            });

            modelBuilder.Entity("RolePermission", b =>
            {
                b.HasOne("GestionUtilisateurs.DataAccess.Models.Permission", null)
                    .WithMany()
                    .HasForeignKey("PermissionsId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("GestionUtilisateurs.DataAccess.Models.Role", null)
                    .WithMany()
                    .HasForeignKey("RolesId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.Role", b =>
            {
                b.Navigation("Permissions");
                b.Navigation("Users");
            });

            modelBuilder.Entity("GestionUtilisateurs.DataAccess.Models.User", b =>
            {
                b.Navigation("AuditLogs");
            });
#pragma warning restore 612, 618
        }
    }
}