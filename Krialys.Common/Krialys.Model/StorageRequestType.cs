using System.Text.Json.Serialization;

namespace Krialys.Data.EF.Model;
/// <summary>
/// Storage request types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StorageRequestType
{
    /// <summary>
    /// Aucune classification spécifique
    /// </summary>
    None = 0,

    /// <summary>
    /// Environnement vierge zippé (\ENV\env_vierge\E0000xx.zip)
    /// </summary>
    EnvironmentEmpty = 1,

    /// <summary>
    /// Ressources (\Ressource\0000xx\fichier01.[csv|xls|...])
    /// </summary>
    EnvironmentResources,

    /// <summary>
    /// Environnement vierge (\ENV\MODRessource\0000xx.[csv|xls|...])
    /// </summary>
    EnvironmentResourcesModel,

    /// <summary>
    /// Qualifs (feux) (\Qualif\0000xx_QUALIF.zip)
    /// </summary>
    QualificationFiles,

    /// <summary>
    /// Fichier de résultats générés par l'ETL (\Resultat\0000xx_RESULT.zip)
    /// </summary>
    ResultFiles,

    /// <summary>
    /// Fichier pour les commandes (\Commande\x\Production_2021_PRS_PRESTA_02.[csv|xls|...])
    /// </summary>
    OrderFiles
}