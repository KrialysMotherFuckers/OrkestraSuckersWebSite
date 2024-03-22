using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Krialys.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TR_SCT_Stream_Category_Type",
                columns: table => new
                {
                    sct_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sct_code = table.Column<string>(type: "TEXT", nullable: false),
                    sct_label = table.Column<string>(type: "TEXT", nullable: true),
                    sct_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TR_SCT_Stream_Category_Type", x => x.sct_id);
                });

            migrationBuilder.CreateTable(
                name: "TM_STF_Storage_File_Request",
                columns: table => new
                {
                    stf_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    stf_fk_origin_id = table.Column<int>(type: "INTEGER", nullable: false),
                    sct_id = table.Column<int>(type: "INTEGER", nullable: false),
                    stf_stream_zipped = table.Column<byte[]>(type: "BLOB", nullable: false),
                    stf_stream_size = table.Column<int>(type: "INTEGER", nullable: false),
                    stf_stream_list = table.Column<string>(type: "TEXT", nullable: false),
                    stf_create_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    stf_create_by = table.Column<string>(type: "TEXT", nullable: false),
                    stf_update_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    stf_update_by = table.Column<string>(type: "TEXT", nullable: true),
                    stf_to_be_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    stf_is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TM_STF_Storage_File_Request", x => x.stf_id);
                    table.ForeignKey(
                        name: "FK_TM_STF_Storage_File_Request_TR_SCT_Stream_Category_Type_sct_id",
                        column: x => x.sct_id,
                        principalTable: "TR_SCT_Stream_Category_Type",
                        principalColumn: "sct_id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TM_STF_Storage_File_Request_sct_id",
                table: "TM_STF_Storage_File_Request",
                column: "sct_id");

            migrationBuilder.Sql($@"
                INSERT INTO TR_SCT_Stream_Category_Type (
                                            sct_active,
                                            sct_label,
                                            sct_code,
                                            sct_id
                                        )
                                        VALUES (
                                            1,
                                            'Fichier(s) de l''Environement vierge lié à une demande',
                                            'EnvironmentEmpty',
                                            {(int)StorageRequestType.EnvironmentEmpty}
                                        ),
                                        (
                                            1,
                                            'Fichier(s) liés à une commande',
                                            'OrderFiles',
                                            {(int)StorageRequestType.EnvironmentResources}
                                        ),
                                        (
                                            1,
                                            'Fichier(s) de Resultat de traitement ETL',
                                            'ResultFiles',
                                            {(int)StorageRequestType.EnvironmentResourcesModel}
                                        ),
                                        (
                                            1,
                                            'Fichier(s) de Qualification lié à une demande',
                                            'QualificationFiles',
                                            {(int)StorageRequestType.QualificationFiles}
                                        ),
                                        (
                                            1,
                                            'Fichier(s) Modèle de Ressource lié à une demande',
                                            'EnvironmentResourcesModel',
                                            {(int)StorageRequestType.ResultFiles}
                                        ),
                                        (
                                            1,
                                            'Fichier(s) de Ressource lié à une demande',
                                            'EnvironmentResources',
                                            {(int)StorageRequestType.OrderFiles}
                                        );
            ");

            migrationBuilder.Sql("PRAGMA journal_mode=\"DELETE\"", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TM_STF_Storage_File_Request");

            migrationBuilder.DropTable(
                name: "TR_SCT_Stream_Category_Type");
        }
    }
}
