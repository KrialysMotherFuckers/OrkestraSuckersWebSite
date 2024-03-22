using System.ComponentModel.DataAnnotations;

/// <summary>

/// </summary>
namespace Krialys.Data.EF.Univers;


public class VDE_DEMANDES_RESSOURCES
{

    public int? TE_ETATID { get; set; }
    [Key]
    public int? TD_DEMANDEID { get; set; }
    [Key]
    public int? TER_ETAT_RESSOURCEID { get; set; }
    public string TER_NOM_FICHIER { get; set; }

    [Key]
    public string TRD_NOM_FICHIER_ORIGINAL { get; set; }
    public string TER_COMMENTAIRE { get; set; }
    public string TER_MODELE_DOC { get; set; }
    public string TER_NOM_MODELE { get; set; }
    public string TRS_FICHIER_OBLIGATOIRE { get; set; }
    public string TER_IS_PATTERN { get; set; }

    public int? TRD_TAILLE_FICHIER { get; set; }

}
