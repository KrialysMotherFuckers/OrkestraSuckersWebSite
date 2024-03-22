using System.ComponentModel.DataAnnotations;

namespace Krialys.Orkestra.Web.Module.ETQ.Models;

public class EtiquetteModel
{
    [Required]
    public string CodeObjEtq { get; set; } // object etiquetté

    [Range(0, 99, ErrorMessage = "The {0} field has invalid range (0-99) or null.")]
    public int? Version { get; set; } // version de l'objet

    public string CodePerimetre { get; set; }

    public string ValDynPerimetre { get; set; }

    public int? DemandeId { get; set; } // champ technique (en simulation ne sert pas)

    public string Source { get; set; } // champ technique

    [Required]
    [Range(typeof(bool), "false", "true")]
    public bool Simulation { get; set; }
}
