using System.ComponentModel.DataAnnotations;

/// <summary>

/// </summary>
namespace Krialys.Data.EF.Univers;


public class VACCGQ_ACCUEIL_GRAPHE_QUALITES
{
    [Key]
    public int QUALIFID { get; set; }

    public string QUALIF_FR { get; set; }

    public string QUALIF_EN { get; set; }

    /* nombre de demandes par  valeur de qualif */
    public int QUALIF_NB { get; set; }

    /* ratio du nb de la valeur de la qualif parmis toutes les qualifs */
    public int RATIO { get; set; }
}
