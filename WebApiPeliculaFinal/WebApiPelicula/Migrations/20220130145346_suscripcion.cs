using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiPelicula.Migrations
{
    public partial class suscripcion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Peticion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LlaveId = table.Column<int>(type: "int", nullable: false),
                    FechaPeticion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peticion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Peticion_LlaveAPI_LlaveId",
                        column: x => x.LlaveId,
                        principalTable: "LlaveAPI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Peticion_LlaveId",
                table: "Peticion",
                column: "LlaveId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Peticion");
        }
    }
}
