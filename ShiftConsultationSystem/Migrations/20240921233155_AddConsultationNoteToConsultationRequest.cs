using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftConsultationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationNoteToConsultationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultationNote",
                table: "ConsultationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultationNote",
                table: "ConsultationRequests");
        }
    }
}
