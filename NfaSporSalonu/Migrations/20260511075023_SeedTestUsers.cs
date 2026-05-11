using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NfaSporSalonu.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "DateOfBirth", "Email", "FailedLoginAttempts", "FirstName", "Gender", "IsActive", "LastName", "LockoutEndTime", "PasswordHash", "PhoneNumber", "ProfileImageUrl", "RoleId" },
                values: new object[,]
                {
                    { 100, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1985, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@nfaspor.com", null, "Ahmet", "Erkek", true, "Yılmaz", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05301000001", null, 1 },
                    { 101, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "elif.kara@nfaspor.com", null, "Elif", "Kadın", true, "Kara", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05302000001", null, 2 },
                    { 102, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1988, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "mehmet.demir@nfaspor.com", null, "Mehmet", "Erkek", true, "Demir", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05302000002", null, 2 },
                    { 103, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "zeynep.aydin@nfaspor.com", null, "Zeynep", "Kadın", true, "Aydın", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05303000001", null, 3 },
                    { 104, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2000, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "can.ozdemir@nfaspor.com", null, "Can", "Erkek", true, "Özdemir", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05303000002", null, 3 },
                    { 105, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1998, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "selin.celik@nfaspor.com", null, "Selin", "Kadın", true, "Çelik", null, "$2a$11$GHZ8eC/IX8DaoGE1PCgFK.V6GRxR7r/yihiiMIW7BFeZMcZg65.Cm", "05303000003", null, 3 }
                });

            migrationBuilder.InsertData(
                table: "UserMemberships",
                columns: new[] { "UserMembershipId", "EndDate", "PlanId", "PurchaseDate", "StartDate", "Status", "UserId" },
                values: new object[,]
                {
                    { 100, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 101 },
                    { 101, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 102 },
                    { 102, new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 103 },
                    { 103, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 104 },
                    { 104, new DateTime(2026, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 105 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserMemberships",
                keyColumn: "UserMembershipId",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "UserMemberships",
                keyColumn: "UserMembershipId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "UserMemberships",
                keyColumn: "UserMembershipId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "UserMemberships",
                keyColumn: "UserMembershipId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "UserMemberships",
                keyColumn: "UserMembershipId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 105);
        }
    }
}
