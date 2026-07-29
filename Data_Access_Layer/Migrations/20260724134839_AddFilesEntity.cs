using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class AddFilesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItem");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Category");

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: true),
                    menuItemId = table.Column<int>(type: "int", nullable: true),
                    reviewId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.id);
                    table.ForeignKey(
                        name: "FK_File_Category_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_File_MenuItem_menuItemId",
                        column: x => x.menuItemId,
                        principalTable: "MenuItem",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_File_Review_reviewId",
                        column: x => x.reviewId,
                        principalTable: "Review",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_File_categoryId",
                table: "File",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_File_FileName",
                table: "File",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_File_FileType",
                table: "File",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_File_menuItemId",
                table: "File",
                column: "menuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_File_reviewId",
                table: "File",
                column: "reviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItem",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Category",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
