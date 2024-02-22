using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewSathi.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LikeCounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LikedBy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LikedBlog = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikeCounts_AspNetUsers_LikedBy",
                        column: x => x.LikedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_LikeCounts_Blogs_LikedBlog",
                        column: x => x.LikedBlog,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LikeCounts_LikedBlog",
                table: "LikeCounts",
                column: "LikedBlog");

            migrationBuilder.CreateIndex(
                name: "IX_LikeCounts_LikedBy",
                table: "LikeCounts",
                column: "LikedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LikeCounts");
        }
    }
}
