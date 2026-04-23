using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HousekeepingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_RoomId_ScheduledDate",
                table: "HousekeepingTasks");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "HousekeepingTasks",
                newName: "LastCleanedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_RoomId_CreatedAt",
                table: "HousekeepingTasks",
                columns: new[] { "RoomId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_RoomId_CreatedAt",
                table: "HousekeepingTasks");

            migrationBuilder.RenameColumn(
                name: "LastCleanedAt",
                table: "HousekeepingTasks",
                newName: "CompletedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "HousekeepingTasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_RoomId_ScheduledDate",
                table: "HousekeepingTasks",
                columns: new[] { "RoomId", "ScheduledDate" });
        }
    }
}
