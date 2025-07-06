using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace slnTestWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false), // this column is non-nullable because of the [Required] attribute
                    DisplayOrder = table.Column<int>(type: "int", nullable: false), // the int type is always non-nullable
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false) // the DateTime type is always non-nullable
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
