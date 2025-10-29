using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task2_HNG13.Migrations
{
    /// <inheritdoc />
    public partial class guhazj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Estimated_gdp",
                table: "Countries",
                type: "real",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Estimated_gdp",
                table: "Countries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
