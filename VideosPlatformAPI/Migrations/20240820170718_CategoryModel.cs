using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VideosPlatformAPI.Migrations
{
    public partial class CategoryModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the Category table first
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            // Insert the default category before adding the CategoryId column to Videos
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                -- Create the default category with title 'Free' if it doesn't exist
                IF NOT EXISTS (SELECT 1 FROM ""Category"" WHERE ""Title"" = 'Free') THEN
                    INSERT INTO ""Category"" (""Id"", ""Title"", ""Color"") VALUES (1, 'Free', '#FFFFFF');
                END IF;
            END
            $$;
            ");

            // Add the CategoryId column to Videos after ensuring the category exists
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Videos",
                type: "integer",
                nullable: false,
                defaultValue: 1);  // Set default value to the existing category Id (1)

            // Assign the default category to any existing videos that don't have a category
            migrationBuilder.Sql(@"
            DO $$
            DECLARE
                default_category_id INT;
            BEGIN
                -- Get the default category ID (which should be 'Free')
                SELECT ""Id"" INTO default_category_id FROM ""Category"" WHERE ""Title"" = 'Free';

                -- Assign the default category to all videos that don't have a category
                UPDATE ""Videos""
                SET ""CategoryId"" = default_category_id
                WHERE ""CategoryId"" = 0 OR ""CategoryId"" IS NULL;
            END
            $$;
            ");

            // Create the foreign key constraint after the CategoryId column and default values are in place
            migrationBuilder.CreateIndex(
                name: "IX_Videos_CategoryId",
                table: "Videos",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Videos_Category_CategoryId",
                table: "Videos",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Videos_Category_CategoryId",
                table: "Videos");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Videos_CategoryId",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Videos");
        }
    }
}
