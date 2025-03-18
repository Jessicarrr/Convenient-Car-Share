using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class AddReturnAreaToBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<int>(
                name: "ReturnAreaId",
                table: "Bookings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReturnAreaId",
                table: "Bookings",
                column: "ReturnAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ParkingAreas_ReturnAreaId",
                table: "Bookings",
                column: "ReturnAreaId",
                principalTable: "ParkingAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);*/

            migrationBuilder.Sql(@"PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM Bookings;

DROP TABLE Bookings;

CREATE TABLE Bookings (
    Id               INTEGER NOT NULL
                             CONSTRAINT PK_Bookings PRIMARY KEY AUTOINCREMENT,
    StartDate        TEXT    NOT NULL,
    EndDate          TEXT    NOT NULL,
    ExtensionDate    TEXT    NOT NULL,
    Price            TEXT    NOT NULL,
    ExtensionPrice   TEXT    NOT NULL,
    UserId           TEXT    NOT NULL,
    CarId            INTEGER NOT NULL,
    activicationCode TEXT,
    ReturnAreaId     INT     REFERENCES ParkingAreas (Id),
    CONSTRAINT FK_Bookings_Cars_CarId FOREIGN KEY (
        CarId
    )
    REFERENCES Cars (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Bookings_AspNetUsers_UserId FOREIGN KEY (
        UserId
    )
    REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

INSERT INTO Bookings (
                         Id,
                         StartDate,
                         EndDate,
                         ExtensionDate,
                         Price,
                         ExtensionPrice,
                         UserId,
                         CarId,
                         activicationCode
                     )
                     SELECT Id,
                            StartDate,
                            EndDate,
                            ExtensionDate,
                            Price,
                            ExtensionPrice,
                            UserId,
                            CarId,
                            activicationCode
                       FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

CREATE INDEX IX_Bookings_CarId ON Bookings (
    ""CarId""
);

            CREATE INDEX IX_Bookings_UserId ON Bookings(
                ""UserId""
            );

            PRAGMA foreign_keys = 1; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ParkingAreas_ReturnAreaId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReturnAreaId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReturnAreaId",
                table: "Bookings");
        }
    }
}
