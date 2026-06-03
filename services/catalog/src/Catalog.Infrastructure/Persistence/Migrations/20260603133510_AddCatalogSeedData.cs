using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2f677466-b64d-4d92-a906-4337c8d71e84"), "Electronics" },
                    { new Guid("2f77fcb1-7f98-49f0-9546-6dd56a8ebf19"), "Books" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAtUtc", "Currency", "Description", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("91ce07a2-b2fe-4de6-a8ef-498625bfedb5"), new Guid("2f677466-b64d-4d92-a906-4337c8d71e84"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", "Noise-cancelling wireless headphones for daily use.", true, "Wireless Headphones", 129.99m },
                    { new Guid("d745a731-6cb4-40fd-a38d-c8ea62e24d4c"), new Guid("2f77fcb1-7f98-49f0-9546-6dd56a8ebf19"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "USD", "A strategic design book for complex software.", true, "Domain-Driven Design", 49.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("91ce07a2-b2fe-4de6-a8ef-498625bfedb5"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d745a731-6cb4-40fd-a38d-c8ea62e24d4c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2f677466-b64d-4d92-a906-4337c8d71e84"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2f77fcb1-7f98-49f0-9546-6dd56a8ebf19"));
        }
    }
}
