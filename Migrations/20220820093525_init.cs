using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatrixIdent.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthItems",
                columns: table => new
                {
                    token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    access_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    token_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    matrix_server_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expires_in = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthItems", x => x.token);
                });

            migrationBuilder.CreateTable(
                name: "EmailValidationItems",
                columns: table => new
                {
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    client_secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    next_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    send_attempt = table.Column<int>(type: "int", nullable: false),
                    sid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    success = table.Column<bool>(type: "bit", nullable: true),
                    expire_after = table.Column<long>(type: "bigint", nullable: true),
                    mxid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailValidationItems", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "HashItems",
                columns: table => new
                {
                    token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    lookup_pepper = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashItems", x => x.token);
                });

            migrationBuilder.CreateTable(
                name: "InvitationRequestItems",
                columns: table => new
                {
                    address = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    medium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    room_alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    room_avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    room_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    room_join_rules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    room_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    room_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sender_avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sender_display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    key = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationRequestItems", x => x.address);
                });

            migrationBuilder.CreateTable(
                name: "Keys",
                columns: table => new
                {
                    identifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    public_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    private_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expiration_timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keys", x => x.identifier);
                });

            migrationBuilder.CreateTable(
                name: "MsisdnValidationItems",
                columns: table => new
                {
                    phone_number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    client_secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    next_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    send_attempt = table.Column<int>(type: "int", nullable: false),
                    sid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    success = table.Column<bool>(type: "bit", nullable: true),
                    expire_after = table.Column<long>(type: "bigint", nullable: true),
                    mxid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MsisdnValidationItems", x => x.phone_number);
                });

            migrationBuilder.CreateTable(
                name: "ThreePidResponseItems",
                columns: table => new
                {
                    address = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    medium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mxid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    not_after = table.Column<long>(type: "bigint", nullable: false),
                    not_before = table.Column<long>(type: "bigint", nullable: false),
                    ts = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreePidResponseItems", x => x.address);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthItems");

            migrationBuilder.DropTable(
                name: "EmailValidationItems");

            migrationBuilder.DropTable(
                name: "HashItems");

            migrationBuilder.DropTable(
                name: "InvitationRequestItems");

            migrationBuilder.DropTable(
                name: "Keys");

            migrationBuilder.DropTable(
                name: "MsisdnValidationItems");

            migrationBuilder.DropTable(
                name: "ThreePidResponseItems");
        }
    }
}
