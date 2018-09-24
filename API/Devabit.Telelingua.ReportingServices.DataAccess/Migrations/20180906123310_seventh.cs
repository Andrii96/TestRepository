using Microsoft.EntityFrameworkCore.Migrations;

namespace Devabit.Telelingua.ReportingServices.DataAccess.Migrations
{
    public partial class seventh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "SqlScripts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "SqlScripts");
        }
    }
}
