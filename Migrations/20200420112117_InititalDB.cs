using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ZudelloThinclientLibrary.Migrations
{
    public partial class InititalDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZCONNECTIONS",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    connection_uuid = table.Column<string>(nullable: false),
                    dataSource = table.Column<string>(nullable: false),
                    InitialCatalog = table.Column<string>(nullable: false),
                    userId = table.Column<string>(nullable: false),
                    password = table.Column<string>(nullable: false),
                    intergrationType = table.Column<string>(nullable: false),
                    zudelloCredentials = table.Column<string>(nullable: true),
                    useIS = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZCONNECTIONS", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ZSETTINGS",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(nullable: false),
                    value = table.Column<string>(nullable: false),
                    Created_at = table.Column<byte[]>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    Updated_at = table.Column<byte[]>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    Deleted_at = table.Column<byte[]>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZSETTINGS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZMAPPING",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: true),
                    DocType = table.Column<string>(nullable: true),
                    Section = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    ProcessOrder = table.Column<long>(type: "int", nullable: true),
                    IsOutgoing = table.Column<long>(type: "TINYINT", nullable: true),
                    IsMasterData = table.Column<long>(type: "TINYINT", nullable: true),
                    connection_id = table.Column<int>(nullable: false),
                    intergration_uuid = table.Column<string>(nullable: true),
                    database = table.Column<string>(nullable: true),
                    Created_at = table.Column<string>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    Updated_at = table.Column<string>(type: "Timestamp DATETIME", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Deleted_at = table.Column<string>(type: "datetime", nullable: true),
                    uuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZMAPPING", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZMAPPING_ZCONNECTIONS_connection_id",
                        column: x => x.connection_id,
                        principalTable: "ZCONNECTIONS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZHASHLOG",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Mapping_id = table.Column<int>(type: "INT", nullable: false),
                    hash = table.Column<byte[]>(type: "byte(16)", nullable: true),
                    Created_at = table.Column<byte[]>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZHASHLOG", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ZHASHLOG_ZMAPPING_Mapping_id",
                        column: x => x.Mapping_id,
                        principalTable: "ZMAPPING",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZLASTSYNC",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Mapping_id = table.Column<int>(type: "INT", nullable: false),
                    lastSync = table.Column<string>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    lastID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZLASTSYNC", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ZLASTSYNC_ZMAPPING_Mapping_id",
                        column: x => x.Mapping_id,
                        principalTable: "ZMAPPING",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZQUEUE",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Mapping_id = table.Column<int>(type: "int", nullable: true),
                    Body = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    connection_id = table.Column<int>(nullable: false),
                    Exception = table.Column<string>(nullable: true),
                    queue_id = table.Column<int>(type: "int", nullable: true),
                    Created_at = table.Column<byte[]>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp"),
                    Updated_at = table.Column<byte[]>(type: "Timestamp DATETIME", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    responseSent = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZQUEUE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZQUEUE_ZCONNECTIONS_connection_id",
                        column: x => x.connection_id,
                        principalTable: "ZCONNECTIONS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ZQUEUE_ZMAPPING_Mapping_id",
                        column: x => x.Mapping_id,
                        principalTable: "ZMAPPING",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZHASHLOG_Mapping_id",
                table: "ZHASHLOG",
                column: "Mapping_id");

            migrationBuilder.CreateIndex(
                name: "IX_ZLASTSYNC_Mapping_id",
                table: "ZLASTSYNC",
                column: "Mapping_id");

            migrationBuilder.CreateIndex(
                name: "IX_ZMAPPING_connection_id",
                table: "ZMAPPING",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_ZQUEUE_connection_id",
                table: "ZQUEUE",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_ZQUEUE_Mapping_id",
                table: "ZQUEUE",
                column: "Mapping_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZHASHLOG");

            migrationBuilder.DropTable(
                name: "ZLASTSYNC");

            migrationBuilder.DropTable(
                name: "ZQUEUE");

            migrationBuilder.DropTable(
                name: "ZSETTINGS");

            migrationBuilder.DropTable(
                name: "ZMAPPING");

            migrationBuilder.DropTable(
                name: "ZCONNECTIONS");
        }
    }
}
