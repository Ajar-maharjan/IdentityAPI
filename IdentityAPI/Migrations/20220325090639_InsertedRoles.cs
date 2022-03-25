using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityAPI.Migrations
{
    public partial class InsertedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "100d9515-d19e-46c4-813c-f9988f14a583", "75970b27-ecef-4cdf-a5f0-d74eb441c38c", "Administrator", "ADMINISTRATOR" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "69cf184a-cf19-4ed2-a70d-72c049a3f1ba", "a899134f-0fa1-4b8e-8274-1979f4eb1f82", "Visitor", "VISITOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "100d9515-d19e-46c4-813c-f9988f14a583");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69cf184a-cf19-4ed2-a70d-72c049a3f1ba");
        }
    }
}
