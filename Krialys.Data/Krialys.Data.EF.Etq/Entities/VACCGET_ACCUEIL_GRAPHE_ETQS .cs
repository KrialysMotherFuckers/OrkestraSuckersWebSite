using System.ComponentModel.DataAnnotations;

/// <summary>

/// </summary>
namespace Krialys.Data.EF.Etq;


public class VACCGET_ACCUEIL_GRAPHE_ETQS
{
    [Key]
    public string PERIODE { get; set; }

    public int NB_ETQ { get; set; }
}