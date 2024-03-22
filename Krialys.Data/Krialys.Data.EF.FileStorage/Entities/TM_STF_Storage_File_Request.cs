using Krialys.Data.EF.Attributes;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.FileStorage
{
    [Table("TM_STF_Storage_File_Request")]
    public partial class TM_STF_StorageFileRequest
    {
        [Key]
        public int stf_id { get; set; }

        [Required]
        public int stf_fk_origin_id { get; set; }

        [Required]
        public StorageRequestType sct_id { get; set; }

        [Required]
        [Column(TypeName = "BLOB")]
        public byte[] stf_stream_zipped { get; set; }

        [Required]
        public int stf_stream_size { get; set; }

        [Required]
        public string stf_stream_list { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        [DefaultDateTimeUTC]
        public DateTime stf_create_date { get; set; } = DateTime.UtcNow;

        [Required]
        public string stf_create_by { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime stf_update_date { get; set; }

        public string stf_update_by { get; set; }

        [Column(TypeName = "boolean")]
        [DefaultValue(false)]
        public bool stf_to_be_deleted { get; set; }

        [Column(TypeName = "boolean")]
        [DefaultValue(false)]
        public bool stf_is_deleted { get; set; }

        [ForeignKey(nameof(sct_id))]
        public virtual TR_SCT_StreamCategoryType TR_SCT_StreamCategoryType { get; set; }
    }
}