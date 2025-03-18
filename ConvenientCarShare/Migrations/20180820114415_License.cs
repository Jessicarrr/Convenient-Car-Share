using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvenientCarShare.Migrations
{
    public partial class License : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)

        {


            migrationBuilder.Sql(@"PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM AspNetUsers;

DROP TABLE AspNetUsers;

CREATE TABLE AspNetUsers (
    Id                   TEXT    NOT NULL
                                 CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    UserName             TEXT,
    NormalizedUserName   TEXT,
    Email                TEXT,
    NormalizedEmail      TEXT,
    EmailConfirmed       INTEGER NOT NULL,
    PasswordHash         TEXT,
    SecurityStamp        TEXT,
    ConcurrencyStamp     TEXT,
    PhoneNumber          TEXT,
    PhoneNumberConfirmed INTEGER NOT NULL,
    TwoFactorEnabled     INTEGER NOT NULL,
    LockoutEnd           TEXT,
    LockoutEnabled       INTEGER NOT NULL,
    AccessFailedCount    INTEGER NOT NULL,
    CreditCardNo         TEXT,
    DOB                  TEXT    NOT NULL
                                 DEFAULT '0001-01-01 00:00:00',
    Name                 TEXT,
    Licence              TEXT
);

INSERT INTO AspNetUsers (
                            Id,
                            UserName,
                            NormalizedUserName,
                            Email,
                            NormalizedEmail,
                            EmailConfirmed,
                            PasswordHash,
                            SecurityStamp,
                            ConcurrencyStamp,
                            PhoneNumber,
                            PhoneNumberConfirmed,
                            TwoFactorEnabled,
                            LockoutEnd,
                            LockoutEnabled,
                            AccessFailedCount,
                            CreditCardNo,
                            DOB,
                            Name,
                            Licence
                        )
                        SELECT Id,
                               UserName,
                               NormalizedUserName,
                               Email,
                               NormalizedEmail,
                               EmailConfirmed,
                               PasswordHash,
                               SecurityStamp,
                               ConcurrencyStamp,
                               PhoneNumber,
                               PhoneNumberConfirmed,
                               TwoFactorEnabled,
                               LockoutEnd,
                               LockoutEnabled,
                               AccessFailedCount,
                               CreditCardNo,
                               DOB,
                               Name,
                               Licence
                          FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

CREATE INDEX EmailIndex ON AspNetUsers (
    'NormalizedEmail'
);

            CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers(
    'NormalizedUserName'
);

            PRAGMA foreign_keys = 1; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
