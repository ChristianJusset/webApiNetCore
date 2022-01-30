using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetCoreApi.Migrations
{
    public partial class comentarioUsuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Comentario",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_UsuarioId",
                table: "Comentario",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentario_AspNetUsers_UsuarioId",
                table: "Comentario",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentario_AspNetUsers_UsuarioId",
                table: "Comentario");

            migrationBuilder.DropIndex(
                name: "IX_Comentario_UsuarioId",
                table: "Comentario");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Comentario");
        }
    }
}
