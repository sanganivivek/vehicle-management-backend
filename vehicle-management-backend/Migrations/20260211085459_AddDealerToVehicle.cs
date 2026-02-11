using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vehicle_management_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DealerId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DealerId",
                table: "Vehicles",
                column: "DealerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Dealers_DealerId",
                table: "Vehicles",
                column: "DealerId",
                principalTable: "Dealers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Dealers_DealerId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_DealerId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DealerId",
                table: "Vehicles");
        }
    }
}
