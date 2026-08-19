using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class fix_estimatetime_value : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_File_Review_reviewId",
                table: "File");

            migrationBuilder.DropIndex(
                name: "IX_File_reviewId",
                table: "File");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EstimatedDeliveryTime",
                table: "Order",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EstimatedDeliveryTime",
                table: "Order",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.CreateIndex(
                name: "IX_File_reviewId",
                table: "File",
                column: "reviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_File_Review_reviewId",
                table: "File",
                column: "reviewId",
                principalTable: "Review",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
