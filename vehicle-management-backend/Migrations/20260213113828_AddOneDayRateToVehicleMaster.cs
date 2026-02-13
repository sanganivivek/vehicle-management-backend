using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vehicle_management_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOneDayRateToVehicleMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OneDayRate",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OneDayRate",
                table: "Vehicles");
        }
    }
}
