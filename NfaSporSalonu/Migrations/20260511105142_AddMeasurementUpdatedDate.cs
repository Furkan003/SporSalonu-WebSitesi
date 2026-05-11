using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NfaSporSalonu.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementUpdatedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "MemberMeasurements",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "MemberMeasurements");
        }
    }
}
