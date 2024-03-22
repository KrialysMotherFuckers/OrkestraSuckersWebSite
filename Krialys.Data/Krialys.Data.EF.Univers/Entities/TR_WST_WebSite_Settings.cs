using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    public partial class TR_WST_WebSite_Settings
    {
        [Key]
        [Column("wst_id")]
        public int Wst_id { get; set; }

        [Column("wst_code")]
        [StringLength(255)]
        public string Wst_Code { get; set; }

        [Column("wst_label")]
        public string Wst_Label { get; set; }

        [Column("wst_value")]
        [Required]
        [StringLength(255)]
        public string Wst_Value { get; set; }

        [Column("wst_description")]
        [StringLength(255)]
        public string Wst_Description { get; set; }
    }

    /// <summary>
    /// Id of the parameters.
    /// </summary>
    public static class ParametersIds
    {
        /// <summary>
        /// Maximum size of a document joined to an order.
        /// </summary>
        public const string Command_DocumentMaxSize = "Command_DocumentMaxSize";

        /// <summary>
        /// List of files extensions allowed for a document joined to an order.
        /// </summary>
        public const string Command_DocumentExtensions = "Command_DocumentExtensions";
    }
}