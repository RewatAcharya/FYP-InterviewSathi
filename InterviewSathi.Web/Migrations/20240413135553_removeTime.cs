using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewSathi.Web.Migrations
{
    /// <inheritdoc />
    public partial class removeTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailableTimeFrames");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvailableTimeFrames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserTime = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    endTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    startTime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableTimeFrames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableTimeFrames_AspNetUsers_UserTime",
                        column: x => x.UserTime,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailableTimeFrames_UserTime",
                table: "AvailableTimeFrames",
                column: "UserTime");
        }
    }
}
