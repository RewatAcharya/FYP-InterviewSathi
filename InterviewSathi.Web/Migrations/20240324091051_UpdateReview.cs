using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewSathi.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PicURL",
                table: "PlatformReviews",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PicURL",
                table: "PlatformReviews");
        }
    }
}
