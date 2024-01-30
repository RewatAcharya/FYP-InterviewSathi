using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewSathi.Web.Migrations
{
    /// <inheritdoc />
    public partial class Friend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SentBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SentTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friends_AspNetUsers_SentBy",
                        column: x => x.SentBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Friends_AspNetUsers_SentTo",
                        column: x => x.SentTo,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_SentBy",
                table: "Friends",
                column: "SentBy");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_SentTo",
                table: "Friends",
                column: "SentTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends");
        }
    }
}
