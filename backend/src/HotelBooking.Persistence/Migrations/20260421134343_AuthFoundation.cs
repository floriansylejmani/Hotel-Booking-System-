using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuthFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seedTimestamp = new DateTime(2026, 4, 21, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: ["Id", "Name", "CreatedAt", "UpdatedAt"],
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Admin", seedTimestamp, null },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Manager", seedTimestamp, null },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Receptionist", seedTimestamp, null },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Housekeeper", seedTimestamp, null },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Guest", seedTimestamp, null }
                });

            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid?>(
                name: "RoleId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "FullName" = TRIM(COALESCE("FirstName", '') || ' ' || COALESCE("LastName", '')),
                    "RoleId" = CASE "Role"
                        WHEN 5 THEN '00000000-0000-0000-0000-000000000001'::uuid
                        WHEN 4 THEN '00000000-0000-0000-0000-000000000002'::uuid
                        WHEN 2 THEN '00000000-0000-0000-0000-000000000003'::uuid
                        WHEN 3 THEN '00000000-0000-0000-0000-000000000004'::uuid
                        ELSE '00000000-0000-0000-0000-000000000005'::uuid
                    END;
                """);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "FullName" = COALESCE(NULLIF("FullName", ''), SPLIT_PART("Email", '@', 1))
                WHERE "FullName" IS NULL OR "FullName" = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId_IsActive",
                table: "Users",
                columns: new[] { "RoleId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "FirstName" = COALESCE(NULLIF("FullName", ''), SPLIT_PART("Email", '@', 1)),
                    "LastName" = '',
                    "PhoneNumber" = '',
                    "Role" = CASE "RoleId"
                        WHEN '00000000-0000-0000-0000-000000000001'::uuid THEN 5
                        WHEN '00000000-0000-0000-0000-000000000002'::uuid THEN 4
                        WHEN '00000000-0000-0000-0000-000000000003'::uuid THEN 2
                        WHEN '00000000-0000-0000-0000-000000000004'::uuid THEN 3
                        ELSE 1
                    END;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId_IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });
        }
    }
}
