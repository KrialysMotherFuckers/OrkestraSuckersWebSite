using System.ComponentModel.DataAnnotations;

/// <summary>

/// </summary>
namespace Krialys.Data.EF.Univers;


public class VACCGD_ACCUEIL_GRAPHE_DEMANDES
{
    [Key]
    public string PERIODE { get; set; }

    public int NB_DEMANDES { get; set; }

    public int NB_REUSSITES { get; set; }

}
