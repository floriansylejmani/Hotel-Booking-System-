using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId",
                table: "HousekeepingTasks");

            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_RoomId",
                table: "HousekeepingTasks");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Status_Type",
                table: "Rooms",
                columns: new[] { "Status", "Type" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_PaidAt",
                table: "Payments",
                columns: new[] { "Status", "PaidAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments",
                sql: "\"Amount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_RoomCharges_NonNegative",
                table: "Invoices",
                sql: "\"RoomCharges\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_TaxAmount_NonNegative",
                table: "Invoices",
                sql: "\"TaxAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_TaxRate_NonNegative",
                table: "Invoices",
                sql: "\"TaxRate\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_TotalAmount_NonNegative",
                table: "Invoices",
                sql: "\"TotalAmount\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId_Status",
                table: "HousekeepingTasks",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_RoomId_ScheduledDate",
                table: "HousekeepingTasks",
                columns: new[] { "RoomId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "RoomId", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId_Status",
                table: "Bookings",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_DateRange",
                table: "Bookings",
                sql: "\"CheckOutDate\" > \"CheckInDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_GuestCount_Positive",
                table: "Bookings",
                sql: "\"GuestCount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_TotalAmount_NonNegative",
                table: "Bookings",
                sql: "\"TotalAmount\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_Status_Type",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_Capacity_Positive",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_Floor_Positive",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Rooms_PricePerNight_NonNegative",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_PaidAt",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payments_Amount_NonNegative",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_RoomCharges_NonNegative",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_TaxAmount_NonNegative",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_TaxRate_NonNegative",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_TotalAmount_NonNegative",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId_Status",
                table: "HousekeepingTasks");

            migrationBuilder.DropIndex(
                name: "IX_HousekeepingTasks_RoomId_ScheduledDate",
                table: "HousekeepingTasks");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_RoomId_CheckInDate_CheckOutDate",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId_Status",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_DateRange",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_GuestCount_Positive",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_TotalAmount_NonNegative",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_AssignedToUserId",
                table: "HousekeepingTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_RoomId",
                table: "HousekeepingTasks",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");
        }
    }
}
