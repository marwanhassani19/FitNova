using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitNova.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AiPlan",
                table: "UserProfiles",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WorkoutLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "WorkoutLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutPlan",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NutritionPlan",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MealType",
                table: "FoodLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "WorkoutLogs");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NutritionPlan",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "MealType",
                table: "FoodLogs");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "UserProfiles",
                newName: "AiPlan");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WorkoutLogs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "WorkoutPlan",
                table: "UserProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
