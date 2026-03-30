using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicTacToe.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameRecords",
                columns: table => new
                {
                    GameRecordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Player1Name = table.Column<string>(type: "TEXT", nullable: false),
                    Player1BoxVal = table.Column<string>(type: "TEXT", nullable: false),
                    Player2Name = table.Column<string>(type: "TEXT", nullable: false),
                    Player2BoxVal = table.Column<string>(type: "TEXT", nullable: false),
                    Draw = table.Column<bool>(type: "INTEGER", nullable: true),
                    BoardState = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentPlayer = table.Column<string>(type: "TEXT", nullable: false),
                    IsOver = table.Column<bool>(type: "INTEGER", nullable: true),
                    Winner = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRecords", x => x.GameRecordId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameRecords");
        }
    }
}
