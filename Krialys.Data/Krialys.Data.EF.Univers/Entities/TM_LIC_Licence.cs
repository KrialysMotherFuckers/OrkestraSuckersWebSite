using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Krialys.Data.EF.Univers
{
    [Table("TM_LIC_Licence")]
    public partial class TM_LIC_Licence
    {
        [Key]
        public int lic_id { get; set; }

        [Required]
        public string lic_product_name { get; set; }

        [Required]
        public string lic_licence_key { get; set; }

        [Required]
        public string lic_issued_to { get; set; }
        public string lic_customer_code { get; set; }
        public string lic_customer_email { get; set; }
        public int lic_licence_type { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime lic_issued_date { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime lic_expiration_date { get; set; }

        [Required]
        [Column(TypeName = "boolean")]
        [DefaultValue(false)]
        public bool lic_is_active { get; set; } = false;
    }
}