using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common.Shared;

public static class ETQ
{
    #region API Catalog
    /// <summary>
    /// Reflects the JSON file that will be created each time for a new Demande
    /// </summary>
    public partial class ApiCatalog
    {
        /// <summary>
        /// ApiUnivers site root
        /// </summary>
        [Required]
        public string WwwRoot { get; set; }

        /// <summary>
        /// API catalog endpoint
        /// </summary>
        [Required]
        public string EndPoint { get; set; }

        /// <summary>
        /// Authentication endpoint
        /// </summary>
        [Required]
        public string Authentication { get; set; }

        [Obsolete("TODO: remove when new catalog API are in prod")] // => ignore (to be deleted as soon as old DTU will be ported to new sauce)
        public string AuthToken { get; set; }

        [Obsolete("TODO: remove when new catalog API are in prod")] // => ignore (to be deleted as soon as old DTU will be ported to new sauce)
        public string Logger { get; set; }
    }

    /// <summary>
    /// Implement / add any new APIs to the catalog here
    /// </summary>
    public partial class ApiCatalog
    {
        #region Etiquettes
        public string EtqCalculate { get; set; }

        public string EtqSuiviRessource { get; set; }

        public string EtqExtraInfoAddon { get; set; }
        #endregion
    }

    #region Multiple labels search based on rules
    /// <summary>
    /// Json payload (POSTed aka INPUT), used as global labels lookup, used as input
    /// See ref. https://krialys.atlassian.net/wiki/spaces/PV2/pages/665911297/Nouvelle+fonction+pour+ApiCatalog+de+recherche+d+tiquettes+et+de+r+cup+ration+d+informations.
    /// </summary>
    public class EtqSearchInput
    {
        /// <summary>
        /// Guid
        /// </summary>
        [Required]
        public string RequestId { get; set; }

        /// <summary>
        /// CodeETQ, numéro d’étiquette
        /// </summary>
        public string LabelCode { get; set; }

        /// <summary>
        /// CodeObjEtq, code objet étiquette
        /// </summary>
        public string LabelObjectCode { get; set; }

        /// <summary>
        /// ValPerimetre, valeur code périmètre
        /// </summary>
        public string PerimeterValue { get; set; }

        /// <summary>
        /// EtqRglCode, code règle
        /// </summary>
        public string RuleCode { get; set; }

        /// <summary>
        /// EtqRglValeur, valeur de règle
        /// </summary>
        public string RuleValue { get; set; }

        /// <summary>
        /// Mode de recherche
        /// </summary>
        public SearchMode? Mode { get; set; }

        /// <summary>
        /// Liste des catalogues souhaités
        /// </summary>
        [Required]
        public IEnumerable<CatalogType> Catalog { get; set; }
    }

    /// <summary>
    /// SearchMode
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SearchMode { Last, All }

    /// <summary>
    /// Catalog (BaseCatalog, GourvernanceRuleCatalog, ResourceCatalog)
    /// </summary>
    //[JsonConverter(typeof(JsonStringEnumMemberConverter))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CatalogType { Base, Rule, Ress }

    /// <summary>
    /// Json (OUPUT), used as global labels lookup, used as output
    /// See ref. https://krialys.atlassian.net/wiki/spaces/PV2/pages/665911297/Nouvelle+fonction+pour+ApiCatalog+de+recherche+d+tiquettes+et+de+r+cup+ration+d+informations.
    /// Value:
    ///<br/>   - RequestId: '001'
    ///<br/>     Label:
    ///<br/>       - LabelCode: 2023_TRF_GRT_01
    ///<br/>         Catalog:
    ///<br/>           - Base:
    ///<br/>               - LabelName: Liste des prestations début 2021
    ///<br/>                 LabelDescription: ''
    ///<br/>                 LabelCreationDate: '2021-12-31T23:00:00'
    ///<br/>             Rule:
    ///<br/>               - RuleCode: null
    ///<br/>                 RuleValue: null
    ///<br/>             Ress:
    ///<br/>               - LabelOutput: null
    ///<br/>                 Input: null
    ///<br/>                 LabelInput: null
    ///<br/>                 LabelLevel: null
    ///<br/>                 LabelObjectCode: null
    /// </summary>
    public class EtqSearchOutput
    {
        public EtqSearchOutput(string requestId, bool success, string errorMessage, int errorCount, IEnumerable<Label> label)
        {
            RequestId = requestId;
            Success = success;
            ErrorMessage = errorMessage;
            ErrorCount = errorCount;
            Label = label;
        }

        /// <summary>
        /// Guid(s)
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Success
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ErrorMessage(s)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ErrorCount - not serialized
        /// </summary>
        [JsonIgnore]
        public int ErrorCount { get; set; }

        /// <summary>
        /// Etiquette(s)
        /// </summary>
        public IEnumerable<Label> Label { get; set; }
    }

    public class Label
    {
        public Label(string labelCode, Catalog catalog)
        {
            LabelCode = labelCode;
            Catalog = catalog;
        }

        /// <summary>
        /// CodeEtq
        /// </summary>
        public string LabelCode { get; set; }

        /// <summary>
        /// Catalogue
        /// </summary>
        public Catalog Catalog { get; set; }
    }

    public class Catalog
    {
        public Catalog(Base @base, IEnumerable<Rule> rule, IEnumerable<Ress> ress)
        {
            Base = @base;
            Rule = rule;
            Ress = ress;
        }

        /// <summary>
        /// Base
        /// </summary>
        public Base Base { get; set; }

        /// <summary>  
        /// Rule - RGD
        /// </summary>
        public IEnumerable<Rule> Rule { get; set; }

        /// <summary>  
        /// Ress
        /// </summary>
        public IEnumerable<Ress> Ress { get; set; }
    }

    public class Base
    {
        public Base(int labelId, string labelPerimeterValue, string labelCode, string labelName, string labelDescription, DateTime? labelCreationDate)
        {
            // Ignored in serialisation, but mandatory to build tree
            #region Not serialized - internal use only
            LabelId = labelId;
            LabelPerimeterValue = labelPerimeterValue;
            LabelCode = labelCode;
            #endregion

            LabelName = labelName;
            LabelDescription = labelDescription;
            LabelCreationDate = labelCreationDate;
        }

        #region Not serialized - internal use
        /// <summary>
        /// EtiquetteId (used internally but not exposed)
        /// </summary>
        [Key]
        [JsonIgnore]
        public int LabelId { get; set; }

        /// <summary>
        /// ValPerimetre (used internally but not exposed)
        /// </summary>
        [Key]
        [JsonIgnore]
        public string LabelPerimeterValue { get; set; }

        /// <summary>
        /// CodeEtq (used internally but not exposed)
        /// </summary>
        [JsonIgnore]
        public string LabelCode { get; set; }
        #endregion

        /// <summary>
        /// LibEtq
        /// </summary>
        public string LabelName { get; set; }
        /// <summary>

        /// DescEtq
        /// </summary>
        public string LabelDescription { get; set; }
        /// <summary>

        /// DateCreateEtQ
        /// </summary>
        [DisplayFormat(DataFormatString = "g")]
        public DateTime? LabelCreationDate { get; set; }
    }

    public class Rule
    {
        public Rule(string ruleCode, string ruleValue)
        {
            RuleCode = ruleCode;
            RuleValue = ruleValue;
        }

        /// <summary>
        /// EtqRglCode
        /// </summary>
        public string RuleCode { get; set; }

        /// <summary>
        /// EtqRglValeur
        /// </summary>
        public string RuleValue { get; set; }
    }

    public class Ress
    {
        public Ress(string labelOutput, string input, string labelInput, int? labelLevel, string labelObjectCode)
        {
            LabelOutput = labelOutput;
            Input = input;
            LabelInput = labelInput;
            LabelLevel = labelLevel;
            LabelObjectCode = labelObjectCode;
        }

        /// <summary>
        /// EtqSortie
        /// </summary>
        public string LabelOutput { get; set; }

        /// <summary>
        /// Entree
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// EtqEntree
        /// </summary>
        public string LabelInput { get; set; }

        /// <summary>
        /// Niveau
        /// </summary>
        public int? LabelLevel { get; set; }

        /// <summary>
        /// CodeObjEtq
        /// </summary>
        public string LabelObjectCode { get; set; }
    }
    #endregion Multiple labels search based on rules

    #endregion API Catalog

    #region ETQ DEMANDE GENERATOR
    /// <summary>
    /// ETQ DEMANDE GENERATOR => OUTPUT
    /// for EtqCalculate & EtqSuiviRessource & ApplyRulesEtq
    /// </summary>
    public class EtqOutput
    {
        /// <summary>
        /// Technical ETL identifier
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Code Etiquette
        /// </summary>
        public string CodeEtq { get; set; }

        /// <summary>
        /// Success flag: true/false
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message returned from Etiquette calculator / or can be an error message returned by the Json deserializer
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Represents the API signature
        /// </summary>
        [Obsolete("TODO: remove when new catalog API are in prod")] // => ignore (to be deleted as soon as old DTU will be ported to new sauce)
        public string Source { get; set; }
    }

    /// <summary>
    /// ETQ DEMANDE GENERATOR => INPUT
    /// </summary>
    public class CalculateEtqInput
    {
        public CalculateEtqInput(string codeObjEtq, int? version, string codePerimetre, string valDynPerimetre, int demandeId, string guid, bool simulation)
        {
            CodeObjEtq = codeObjEtq;
            Version = version;
            CodePerimetre = codePerimetre;
            ValDynPerimetre = valDynPerimetre;
            DemandeId = demandeId;
            Guid = guid;
            Simulation = simulation;
        }

        /// <summary>
        /// Code objet étiquette
        /// </summary>
        [Required]
        public string CodeObjEtq { get; set; }

        /// <summary>
        /// Version de l'étiquette, peut être égale à null si pas de valeur à passer
        /// </summary>
        public int? Version { get; set; }

        /// <summary>
        /// Code périmètre, peut être égal à null ou à "" si pas de valeur à passer
        /// </summary>
        public string CodePerimetre { get; set; }

        /// <summary>
        /// Valeur dynamique du périmetre - on s'attend à avoir un code périmètre renseigné
        /// </summary>
        public string ValDynPerimetre { get; set; }

        /// <summary>
        /// Demande en cours (variable globale inexistante, cf. Seb)
        /// </summary>
        [Required]
        public int DemandeId { get; set; }

        /// <summary>
        /// Technical ETL identifier
        /// </summary>
        [Required]
        public string Guid { get; set; }

        /// <summary>
        /// Simulation d'une étiquette (true), par défaut on la crée (false) en base
        /// </summary>
        public bool Simulation { get; set; } // OB-340 (ne pas défaulter à true)
    }
    #endregion

    /// <summary>
    /// Transverse class between TETQ_ETIQUETTES and TD_DEMANDES.
    /// Used in DataShop.
    /// </summary>
    public class EtiquetteDetails
    {
        [Key]
        public int TETQ_ETIQUETTEID { get; set; }

        public string TETQ_CODE { get; set; }

        public string TETQ_LIB { get; set; }

        [DisplayFormat(DataFormatString = "g")]
        public DateTime TETQ_DATE_CREATION { get; set; }

        public string DEMANDEUR { get; set; }

        public string REFERENT { get; set; }
    }

    ///// <summary>
    ///// Structure associée au mécanisme interne de creation de code etiquette
    ///// </summary>
    public class Etq
    {
        public string Code { get; set; }

        public string OrdreField { get; set; }

        public int OrdreAssemblage { get; set; }

        public string Val { get; set; }
    }

    public class EtqSuiviRessourceFile
    {
        public IList<EtqSuiviRessourceFileRaw> ListEtqSuiviRessourceFileRaw { get; set; } //contenu du fichier de suivi de ressources/entrée d'etiquette

        public EtqSuiviRessourceFile()
        {
            ListEtqSuiviRessourceFileRaw = new List<EtqSuiviRessourceFileRaw>();
        }
    }

    /// <summary>
    /// Structure associée au contenu d'un fichier de ressource d'étiquette => INPUT
    /// </summary>
    public class EtqSuiviRessourceFileRaw
    {
        public EtqSuiviRessourceFileRaw(string guid, string codeETQ, string entree, string typeRessource, string valeurEntree)
        {
            Guid = guid;
            CodeETQ = codeETQ;
            Entree = entree;
            TypeRessource = typeRessource;
            ValeurEntree = valeurEntree;
        }

        /// <summary>
        /// Technical ETL identifier
        /// </summary>
        [Required]
        public string Guid { get; set; }

        /// <summary>
        /// Code Etiquette
        /// </summary>
        //[JsonPropertyName("CodeEtq")] TODO lorsque bascule catalogue API
        [Required]
        public string CodeETQ { get; set; }

        /// <summary>
        /// Informatif : contexte
        /// </summary>
        public string Entree { get; set; }

        /// <summary>
        /// Type de ressource
        /// </summary>
        public string TypeRessource { get; set; }

        /// <summary>
        /// Valeur de la ressource
        /// </summary>
        public string ValeurEntree { get; set; }
    }

    public class EtqExtraInfoAddonFile
    {
        public IList<EtqExtraInfoAddonFileRaw> ListEtqExtraInfoAddonFileRaw { get; set; } //contenu du fichier de suivi de ressources/entrée d'etiquette

        public EtqExtraInfoAddonFile()
        {
            ListEtqExtraInfoAddonFileRaw = new List<EtqExtraInfoAddonFileRaw>();
        }
    }

    /// <summary>
    /// Structure associée au contenu d'un fichier de ressource d'etiquette => INPUT
    /// </summary>
    public class EtqExtraInfoAddonFileRaw
    {
        public EtqExtraInfoAddonFileRaw(string guid, string codeETQ, string etqLib, string etqDesc)
        {
            Guid = guid;
            CodeETQ = codeETQ;
            EtqLib = etqLib;
            EtqDesc = etqDesc;
        }

        /// <summary>
        /// Technical ETL identifier
        /// </summary>
        [Required]
        public string Guid { get; set; }

        /// <summary>
        /// Code Etiquette
        /// </summary>
        //[JsonPropertyName("CodeEtq")] TODO lorsque bascule catalogue API
        [Required]
        public string CodeETQ { get; set; }

        /// <summary>
        /// Libellé Etiquette
        /// </summary>
        public string EtqLib { get; set; }

        /// <summary>
        /// Description de l'étiquette
        /// </summary>
        public string EtqDesc { get; set; }
    }

    /// <summary>
    /// URL test #1 : /// ETQ RULES  => INPUT
    /// </summary>
    public class EtqRules
    {
        public string ActeurId { get; set; }

        public int TETQ_ETIQUETTEID { get; set; }

        public int TETQR_ETQ_REGLEID { get; set; }

        public int TRGLRV_REGLES_VALEURID_PARENT { get; set; } // id d origine

        public int TRGLRV_REGLES_VALEURID { get; set; } // id souhaité 

        public int TOBJE_OBJET_ETIQUETTEID { get; set; }

        public string COMMENT { get; set; }
    }
}