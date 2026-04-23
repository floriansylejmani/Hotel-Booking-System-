using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RoomModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_Capacity_Positive",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_Floor_Positive",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_PricePerNight_NonNegative",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Rooms",
                newName: "RoomNumber");

            migrationBuilder.RenameColumn(
                name: "Floor",
                table: "Rooms",
                newName: "FloorNumber");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Rooms",
                newName: "BedCount");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_Number",
                table: "Rooms",
                newName: "IX_Rooms_RoomNumber");

            migrationBuilder.AddColumn<string[]>(
                name: "Amenities",
                table: "Rooms",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_BedCount_Positive",
                table: "Rooms",
                sql: "\"BedCount\" >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_FloorNumber_NonNegative",
                table: "Rooms",
                sql: "\"FloorNumber\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_PricePerNight_Positive",
                table: "Rooms",
                sql: "\"PricePerNight\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_BedCount_Positive",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_FloorNumber_NonNegative",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_PricePerNight_Positive",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "RoomNumber",
                table: "Rooms",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "FloorNumber",
                table: "Rooms",
                newName: "Floor");

            migrationBuilder.RenameColumn(
                name: "BedCount",
                table: "Rooms",
                newName: "Capacity");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_RoomNumber",
                table: "Rooms",
                newName: "IX_Rooms_Number");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rooms",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_Capacity_Positive",
                table: "Rooms",
                sql: "\"Capacity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_Floor_Positive",
                table: "Rooms",
                sql: "\"Floor\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rooms_PricePerNight_NonNegative",
                table: "Rooms",
                sql: "\"PricePerNight\" >= 0");
        }
    }
}
