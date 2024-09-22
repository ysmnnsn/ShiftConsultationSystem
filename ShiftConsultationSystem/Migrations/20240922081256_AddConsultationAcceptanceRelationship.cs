using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftConsultationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationAcceptanceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultationRequestId1",
                table: "ConsultationAcceptances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAcceptances_ConsultationRequestId1",
                table: "ConsultationAcceptances",
                column: "ConsultationRequestId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationAcceptances_ConsultationRequests_ConsultationRequestId1",
                table: "ConsultationAcceptances",
                column: "ConsultationRequestId1",
                principalTable: "ConsultationRequests",
                principalColumn: "ConsultationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationAcceptances_ConsultationRequests_ConsultationRequestId1",
                table: "ConsultationAcceptances");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationAcceptances_ConsultationRequestId1",
                table: "ConsultationAcceptances");

            migrationBuilder.DropColumn(
                name: "ConsultationRequestId1",
                table: "ConsultationAcceptances");
        }
    }
}
