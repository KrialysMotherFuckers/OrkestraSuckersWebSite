using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.FileStorage
{
    [Table("TR_SCT_Stream_Category_Type")]
    public partial class TR_SCT_StreamCategoryType
    {
        [Key]
        public int sct_id { get; set; }

        [Required]
        public string sct_code { get; set; }

        public string sct_label { get; set; }

        [Column(TypeName = "boolean")]
        [DefaultValue(false)]
        public bool sct_active { get; set; }
    }
}