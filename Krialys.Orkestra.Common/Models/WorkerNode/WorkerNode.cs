using Krialys.Common.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common.Models.WorkerNode;

public static class WorkerNodeExtensions
{
    /// <summary>
    /// Recycle a CPU with default factory settings, but: Id, Name and Machine parameters are not erased
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns></returns>
    public static WorkerNode RecycleCpu(this WorkerNode cpu)
    {
        // Check CpuDemandes
        if (cpu?.Demande is null)
        {
            return cpu;
        }

        // Infos au niveau CPU
        cpu.IsFree = true;
        cpu.Demande.DemandeId = null;
        cpu.Demande.EtatId = null;
        cpu.Demande.JsonData = null;

        // Reset Step Number
        //cpu.StepNumber = 0;

        cpu.StepWorkflow = null;
        cpu.StepStartedAt = null;

        cpu.LastCmdResult = null;
        cpu.LastCmdError = null;

        // Now au niveau global
        //cpu.StartedAt = DateExtensions.GetUtcNow();

        //Now au niveau Demande
        cpu.Demande.StartedAt = DateExtensions.GetUtcNowSecond();

        cpu.Resilience = false; // a true uniquement si il y a eu reconnexion apres coupure entre central et client

        // Clear Batchs list
        cpu.Demande.ListBatchsAttributes.Clear();

        // Cleanr Ressources list (user files for specific "demande")
        cpu.Demande.ListRessourcesAttributes.Clear();

        // Clear Qualifications list
        cpu.Demande.IsValidQualifAttributes = null;

        // Clear Infos de paramètrage liées au CPU client (ex : remplacement à faire dans les bat sur la base de mot clef)
        cpu.Demande.ListParamAttributes.Clear();

        return cpu;
    }

    public static WorkerNode ClearPartOfCpu(this WorkerNode cpu)
    {
        if (cpu?.Demande is null) return cpu;

        cpu.LastCmdResult = null;
        cpu.LastCmdError = null;

        return cpu;
    }
}

/// <summary>
/// ParallelU Client side - (connection manager) -
/// Each CPU instance (cpuName mapped to cpuMap) can have 1 to N connectionId
/// Each CPU connection can run if a Client function/parameters call (known from Server), or can be an Http route/parameters call from URL are given
/// </summary>
public class WorkerNode : WorkerNodeExt
{
    #region Properties

    public string Id { get; set; }                     // Socket connection Id ("zY9I3JiavAMkkUsBqRTszg"), equivalent of 'a PK for a CPU instance'

    public string Name { get; set; }                   // Connection Client name ("CPU-ALTX")

    public string Machine { get; set; }                // Machine name from where CPU is running

    public bool? IsFree { get; set; }                  // Flag indicating if the client is ready for executing environment job (the client will set this value back)

    public DateTime? StartedAt { get; set; }            // Local date time since CPU has been started

    public bool? LastCmdResult { get; set; }           // Result from an executed command, null means not started, false means error and true means success

    public string LastCmdError { get; set; }           // Error message from an executed command, if any (should be set to null before using it)

    public Demandes Demande { get; set; }              // Class that holds Demande attributes

    public string StepWorkflow { get; set; }           // Step of worflow for process one "Demande"

    public int StepNumber { get; set; }                // n° Step of workflow for process one "Demande"

    public bool Resilience { get; set; }               // Resilience flag => when a CPU has a new Socket Id and when a demande is not yet complete, then CPU send its last step including this flag that must be set to true

    public DateTime? StepStartedAt { get; set; }       // Start time of current Step

    public DateTime? LastJobRun { get; set; }          // Last job execution time

    public Authorizations Authorization { get; set; }

    /// <summary>
    /// Size in KB of the buffer used while transporting datas (Download/Upload)
    /// </summary>
    public const int BufferSize = 256 * 1024;

    #endregion

    /// <summary>
    /// Default constructor initializes CPUDemandes
    /// </summary>
    public WorkerNode() => Init();

    /// <summary>
    /// This constructor initializes an expected default CPU structure with CPUDemandes initialized
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="name"></param>
    /// <param name="machine"></param>
    public WorkerNode(string connectionId, string name, string machine) : base()
    {
        Init();

        Id = connectionId;
        Name = name;
        Machine = machine;
    }

    private void Init()
    {
        // Defaulted values
        IsFree = true;
        LastCmdResult = null;
        LastCmdError = null;
        StepWorkflow = null;
        StartedAt = DateTime.UtcNow;
        StepStartedAt = null;
        Demande = new Demandes();
        Authorization = new Authorizations();
        StepNumber = 0;
        Resilience = false;
    }

}

/// <summary>
/// Check if CPU is authorized
/// </summary>
public class Authorizations
{
    public bool IsAuthorized { get; set; }
    public string Message { get; set; }
    public string Action { get; set; }
}

/// <summary>
/// Demandes attributes
/// </summary>
public class Demandes
{
    public Demandes()
    {
        ListRessourcesAttributes = new List<RessourcesAttributes>();
        ListBatchsAttributes = new List<BatchsAttributes>();
        ListParamAttributes = new List<ParamAttributes>();
    }

    public int? DemandeId { get; set; }                 // DemandeId (TODO: if necessary, add other parameters required by CPU Controller below)
    public int? EtatId { get; set; }                    // Application involded by this "Demande"
    public bool? IsValidQualifAttributes { get; set; }  // Chk Qualif content file (file generated by ETL) (null == means the qualif file was optional, false means the file is not compliant, true the qualification structure is OK)
    public DateTime? StartedAt { get; set; } = null;           // Local date time since CPU has start the specific Demand
    public byte[] JsonData { get; set; }                // Serialized data exchanged between CPU and Service used for MappingQualifDemande + TTL_LOGS
    public IList<BatchsAttributes> ListBatchsAttributes { get; set; }         // Batchs file list
    public IList<RessourcesAttributes> ListRessourcesAttributes { get; set; } // Ressource file list requiered for check correct send
    public IList<ParamAttributes> ListParamAttributes { get; set; }           // Batchs file list
}

/// <summary>
/// Ressources attributes
/// </summary>
public class RessourcesAttributes
{
    public int DemandeOrigine { get; set; }      // Used to retrieve right path
    public string NomFichier { get; set; }       // Name of the source file
    public string Destination { get; set; }      // Name of the target sub-directory
    public bool IsTransfertStarted { get; set; } // Flag for know which file we try to sent to client
}

/// <summary>
/// Batchs attributes
/// </summary>
public class BatchsAttributes
{
    public int OrdreExecution { get; set; } // Lead batch file execution order
    public string NomFichier { get; set; }  // Name of the Batch file
    public bool SubmitExec { get; set; }    // Flag for know which cmd to execute was transmited to client
    public int? ExitCode { get; set; }      // Exit code from the batch file (if null means a problem occurred if was supposed to be executed)
    public int? ProcessId { get; set; }     // Get ProcessId, useful if we want to kill a process that runs infinitely
}

/// <summary>
/// using var instance = ExcelReader.CreateInstance();
/// var queryableQualif = await instance.Load<MappingQualif>(@"C:\KRepertoireTravail_DEV\Qualif.csv", "");
/// error = instance.GetLastError();
/// </summary>
public class MappingQualifDemande
{  // TODO  max length
   //[Required(ErrorMessage = "Champ obligatoire (3 caractères min, 255 max)")]
   //[StringLength(255), MinLength(1), MaxLength(255)]

    [Required(ErrorMessage = "CODE_QUALIF Champ obligatoire (30 max)")]
    [StringLength(30), MinLength(1), MaxLength(30)]
    public string CODE_QUALIF { get; set; }
    public string NOM_QUALIF { get; set; }
    public string CODE_ETAT_QUALIF { get; set; }
    [Required(ErrorMessage = "VALEUR_QUALIF Champ obligatoire")]
    public int VALEUR_QUALIF { get; set; }
    public string NATURE_QUALIF { get; set; }
    public string DATASET_QUALIF { get; set; }
    public string OBJET_QUALIF { get; set; }
    // Ne prendre que les x caracteres qui peuvent rentrer en bdd, on s autorise a tronquer à 255 caractères
    private string _COMMENT_QUALIF = string.Empty;
    public string COMMENT_QUALIF
    {
        get => _COMMENT_QUALIF;
        set => _COMMENT_QUALIF = value.Length < 255 ? value : value[..255];
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    // non exploité en bdd => on l'ignore
    public object DATE_PROD_QUALIF { get; set; }
    public int ID_DEMANDE { get; set; }
}

public class ParamAttributes
{
    public string Key { get; set; }
    public string Value { get; set; }
}
