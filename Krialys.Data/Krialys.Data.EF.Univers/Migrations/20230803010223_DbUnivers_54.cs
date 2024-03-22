using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class DbUnivers_54 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "TRST_STATUTID_OLD",
            //    table: "TE_ETATS",
            //    type: "TEXT",
            //    maxLength: 3,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldMaxLength: 3);

            migrationBuilder.AddColumn<string>(
                name: "lic_customer_code",
                table: "TM_LIC_Licence",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lic_customer_email",
                table: "TM_LIC_Licence",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lic_licence_type",
                table: "TM_LIC_Licence",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                INSERT INTO TM_LIC_Licence (
                                               lic_is_active,
                                               lic_expiration_date,
                                               lic_issued_date,
                                               lic_issued_to,
                                               lic_licence_key,
                                               lic_product_name
                                           )
                                           VALUES (
                                               1,
                                               '2023-10-03 23:43:09',
                                               '2023-08-03 00:59:48.2851617',
                                               '',
                                               'PExpY2Vuc2U-DQogIDxJZD4zYWY3YzlkOC1mZmMxLTRkMDUtOTc1Mi1jZmYxNzQ0YmFiYTc8L0lkPg0KICA8VHlwZT5TdGFuZGFyZDwvVHlwZT4NCiAgPEV4cGlyYXRpb24-V2VkLCAwMiBPY3QgMjAyNCAxMzowMDo0NiBHTVQ8L0V4cGlyYXRpb24-DQogIDxDdXN0b21lcj4NCiAgICA8TmFtZT48L05hbWU-DQogIDwvQ3VzdG9tZXI-DQogIDxTaWduYXR1cmU-TUVRQ0lEYXdaaHlqbTlRaTZTZGRGdzdCMlIzL1FSSHpIVlJSR29NV0VFSzhFdjVhQWlBb1FqWjBWUWdacUJGd2JtaTBDaW9RdVF6dG5PK0xvQTIrUXdKbThCWTNNdz09PC9TaWduYXR1cmU-DQo8L0xpY2Vuc2U-',
                                               ''
                                           );

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "TRST_STATUTID_OLD",
            //    table: "TE_ETATS",
            //    type: "TEXT",
            //    maxLength: 3,
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldMaxLength: 3,
            //    oldNullable: true);

            migrationBuilder.DropColumn(
                name: "lic_customer_code",
                table: "TM_LIC_Licence");

            migrationBuilder.DropColumn(
                name: "lic_customer_email",
                table: "TM_LIC_Licence");

            migrationBuilder.DropColumn(
                name: "lic_licence_type",
                table: "TM_LIC_Licence");
        }
    }
}
