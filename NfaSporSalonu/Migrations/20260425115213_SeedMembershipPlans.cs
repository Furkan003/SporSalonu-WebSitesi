using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NfaSporSalonu.Migrations
{
    /// <inheritdoc />
    public partial class SeedMembershipPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MembershipPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.InsertData(
                table: "MembershipPlans",
                columns: new[] { "PlanId", "Category", "Description", "DurationInDays", "IsActive", "PlanName", "Price" },
                values: new object[,]
                {
                    { 1, "Öğrenci", "Öğrenci belgesi ile geçerlidir.", 30, true, "Öğrenci 1 Ay", 1100m },
                    { 2, "Öğrenci", "Öğrenci belgesi ile geçerlidir.", 90, true, "Öğrenci 3 Ay", 3000m },
                    { 3, "Öğrenci", "Öğrenci belgesi ile geçerlidir. En çok tercih edilen paket!", 180, true, "Öğrenci 6 Ay", 5500m },
                    { 4, "Öğrenci", "Öğrenci belgesi ile geçerlidir.", 365, true, "Öğrenci 12 Ay", 9000m },
                    { 5, "Sivil", "Tüm spor salonu hizmetlerini kapsar.", 30, true, "Sivil 1 Ay", 1200m },
                    { 6, "Sivil", "Tüm spor salonu hizmetlerini kapsar.", 90, true, "Sivil 3 Ay", 3250m },
                    { 7, "Sivil", "Tüm spor salonu hizmetlerini kapsar. En çok tercih edilen paket!", 180, true, "Sivil 6 Ay", 6000m },
                    { 8, "Sivil", "Tüm spor salonu hizmetlerini kapsar.", 365, true, "Sivil 12 Ay", 10000m },
                    { 9, "Pilates", "Reformer Pilates – 8 seanslık paket.", 60, true, "Pilates 8 Seans", 2500m },
                    { 10, "Pilates", "Reformer Pilates – 24 seanslık paket. En çok tercih edilen paket!", 180, true, "Pilates 24 Seans", 7200m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "MembershipPlans",
                keyColumn: "PlanId",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "MembershipPlans");
        }
    }
}
