using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AppDbInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecieverId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSeen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Error = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    DevCode = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lookups_Lookups_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Lookups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: true),
                    ProcessingLock = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastProcessingAttempt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LookupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevCode = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LookupDetails_LookupDetails_ParentId",
                        column: x => x.ParentId,
                        principalTable: "LookupDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LookupDetails_Lookups_LookupId",
                        column: x => x.LookupId,
                        principalTable: "Lookups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RouterLink = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tooltip = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false),
                    MenuTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMenus_LookupDetails_MenuTypeId",
                        column: x => x.MenuTypeId,
                        principalTable: "LookupDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", maxLength: 100, nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AppMenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenus_AppMenus_AppMenuId",
                        column: x => x.AppMenuId,
                        principalTable: "AppMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_Label",
                table: "AppMenus",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_MenuTypeId",
                table: "AppMenus",
                column: "MenuTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_ParentId",
                table: "AppMenus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupDetails_LookupId",
                table: "LookupDetails",
                column: "LookupId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupDetails_LookupId_Name",
                table: "LookupDetails",
                columns: new[] { "LookupId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LookupDetails_ParentId",
                table: "LookupDetails",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_DevCode",
                table: "Lookups",
                column: "DevCode");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_Name",
                table: "Lookups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_ParentId",
                table: "Lookups",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenus_AppMenuId",
                table: "RoleMenus",
                column: "AppMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenus_RoleId_AppMenuId",
                table: "RoleMenus",
                columns: new[] { "RoleId", "AppMenuId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNotifications");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "RoleMenus");

            migrationBuilder.DropTable(
                name: "AppMenus");

            migrationBuilder.DropTable(
                name: "LookupDetails");

            migrationBuilder.DropTable(
                name: "Lookups");
        }
    }
}
