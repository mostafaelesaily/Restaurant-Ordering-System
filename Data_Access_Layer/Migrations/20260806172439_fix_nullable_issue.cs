using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data_Access_Layer.Migrations
{
    /// <inheritdoc />
    public partial class fix_nullable_issue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderCoupon");

            migrationBuilder.AddColumn<int>(
                name: "couponId",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Order_couponId",
                table: "Order",
                column: "couponId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Coupon_couponId",
                table: "Order",
                column: "couponId",
                principalTable: "Coupon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Coupon_couponId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_couponId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "couponId",
                table: "Order");

            migrationBuilder.CreateTable(
                name: "OrderCoupon",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCoupon", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderCoupon_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderCoupon_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderCoupon_CouponId",
                table: "OrderCoupon",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCoupon_OrderId",
                table: "OrderCoupon",
                column: "OrderId");
        }
    }
}
