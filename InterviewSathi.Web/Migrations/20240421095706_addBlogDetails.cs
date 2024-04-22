using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewSathi.Web.Migrations
{
    /// <inheritdoc />
    public partial class addBlogDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Blogs");
        }
    }
}
