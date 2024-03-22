namespace Krialys.Common.Literals;

/// <summary>
/// UTDs statuses used for production
/// </summary>
public static class StatusLiteral
{
    /// <summary>
    /// Actif
    /// </summary>
    public const string Available = "A";

    /// <summary>
    /// Annulation en cours
    /// </summary>
    public const string Stopping = "AD";

    /// <summary>
    /// Demande archivée
    /// </summary>
    public const string ArchivedRequest = "AR";

    /// <summary>
    /// Brouillon
    /// </summary>
    public const string Draft = "B";

    /// <summary>
    /// Demande annulée
    /// </summary>
    public const string CanceledRequest = "DA";

    /// <summary>
    /// Mode Brouillon
    /// </summary>
    public const string DraftMode = "DB";

    /// <summary>
    /// Demande créée et attente d'exécution
    /// </summary>
    public const string CreatedRequestAndWaitForExecution = "DC";

    /// <summary>
    /// Demande programmée
    /// </summary>
    public const string ScheduledRequest = "DP";

    /// <summary>
    /// Demande Réalisée
    /// </summary>
    public const string RealizedRequest = "DR";

    /// <summary>
    /// En cours d'exécution
    /// </summary>
    public const string InProgress = "EC";

    /// <summary>
    /// Erreur d'exécution
    /// </summary>
    public const string InError = "ER";

    /// <summary>
    /// Inactif
    /// </summary>
    public const string Deactivated = "I";

    /// <summary>
    /// Résultats Invalidés
    /// </summary>
    public const string InvalidatedRequest = "IV"; // not used in code yet

    /// <summary>
    /// Modèle pour planification
    /// </summary>
    public const string ScheduleModel = "MO";

    /// <summary>
    /// Delai d'attente prérequis dépassé
    /// </summary>
    public const string WaitingTriggerFileTimeout = "NF";

    /// <summary>
    /// Résultats validés
    /// </summary>
    public const string ValidatedResult = "VA";

    /// <summary>
    /// Attente fichier déclencheur
    /// </summary>
    public const string WaitingTriggerFile = "WF";

    /// <summary>
    /// Prototype
    /// </summary>
    public const string Prototype = "P";

    /// <summary>
    /// Annulé
    /// </summary>
    public const string Canceled = "C";

    /// <summary>
    /// Planif Annulée
    /// </summary>
    public const string PlanningCancelled = "PA";
   
    /// <summary>
    /// Valeur Oui
    /// </summary>
    public const string Yes = "O";

    /// <summary>
    /// Valeurs non
    /// </summary>
    public const string No = "N";
}

public static class OrderStatus
{
    /// <summary>
    /// Order Handled
    /// </summary>
    public const string Handled = "PROGRES_COM";

    /// <summary>
    /// Order delivered
    /// </summary>
    public const string Delivered = "LIVRE_COM";

    /// <summary>
    /// Order closed
    /// </summary>
    public const string Closed = "FIN_COM";

    /// <summary>
    /// Order Produced
    /// </summary>
    public const string EndOfProduction = "FIN_PROD";

    /// <summary>
    /// Order rejected
    /// </summary>
    public const string Rejected = "REJET_COM";
    
    /// <summary>
    /// Order cancelled
    /// </summary>
    public const string Canceled = "ANNUL_COM";
}