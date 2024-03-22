using System.ComponentModel.DataAnnotations;

/// <summary>
/// Créer les modèles dans le répertoire "Entities"
/// NB : Les classes EF seront à placer ici
/// </summary>
namespace Krialys.Data.EF.Univers;

public class VSCU_CTRL_STRUCTURE_UPLOADS
{
    [Key]
    [Display(Name = "TE_ETATID")]
    public int TE_ETATID { get; set; }

    [Key]
    [Display(Name = "TLEM_ACTION")]
    public string TLEM_ACTION { get; set; }

    [Key]
    [Display(Name = "TLEM_FILE_TYPE")]
    public string TLEM_FILE_TYPE { get; set; }

    [Key]
    [Display(Name = "TLEM_PATH_NAME")]
    public string TLEM_PATH_NAME { get; set; }

    [Key]
    [Display(Name = "TLEM_FILENAME_PATTERN")]
    public string TLEM_FILENAME_PATTERN { get; set; }
}
