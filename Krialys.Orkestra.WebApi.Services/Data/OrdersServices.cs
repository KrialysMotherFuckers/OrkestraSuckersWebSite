using Krialys.Common.Extensions;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Krialys.Orkestra.WebApi.Services.Data;

public interface IOrdersServices : IScopedService
{
    Task<IActionResult> DeleteOrderAsync(int id, string userId);

    Task<IActionResult> ChangeOrderPhaseAsync(int id, string userId, ChangeOrderPhaseArguments args);

    Task<IActionResult> DuplicateOrderAsync(int id, string userId);

    Task<IActionResult> SaveDocumentAsync(IList<IFormFile> UploadFiles, int orderId, string userId);

    Task<IActionResult> RemoveDocumentAsync(string UploadFiles, int orderId, string userId);

    IActionResult ReadAssociatedProductions(int orderId);
}

public class OrdersServices : IOrdersServices
{
    #region Injected services
    /// <summary>
    /// Session with the Univers database.
    /// </summary>
    private readonly KrialysDbContext _dbContext;

    /// <summary>
    /// Application configuration properties.
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Injected service which performs logging.
    /// </summary>
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Injected service which triggers and tracks events shared with the client.
    /// </summary>
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersServices"/> class.
    /// </summary>
    /// <param name="dbContext">Session with the Univers database.</param>
    /// <param name="configuration">Application configuration properties.</param>
    /// <param name="logger">Service which performs logging.</param>
    /// <param name="trackedEntitiesServices">Injected service which triggers and tracks events shared with the client.</param>
    /// <exception cref="ArgumentNullException">Dependency injection failed.</exception>
    public OrdersServices(KrialysDbContext dbContext, IConfiguration configuration,
        Serilog.ILogger logger, ITrackedEntitiesServices trackedEntitiesServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trackedEntitiesServices = trackedEntitiesServices ?? throw new ArgumentNullException(nameof(trackedEntitiesServices));
    }
    #endregion

    #region Document methods (privates)
    /// <summary>
    /// Gets the path where the document is saved.
    /// </summary>
    /// <param name="orderId">Id of the order (TCMD_COMMANDEID)</param>
    /// <param name="documentName">Name under which the document is saved.</param>
    private string GetDocumentPath(int orderId, string documentName)
        => Path.Combine(_configuration["ParallelU:PathCommande"] ?? string.Empty,
            orderId.ToString(),
            documentName);

    /// <summary>
    /// Delete a document attached to an order.
    /// Delete the file and the related entry in the database.
    /// </summary>
    /// <param name="document">Entry in the document table representing the document to delete.</param>
    private Task DeleteDocumentAsync(TCMD_DOC_DOCUMENTS document)
    {
        // Path to the file.
        string filePath = Path.Combine(_configuration["ParallelU:PathCommande"] ?? string.Empty,
            document.TCMD_COMMANDEID.ToString(),
            document.TCMD_DOC_FILENAME);

        // If file exists, delete it.
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Delete documents entries in database.
        _dbContext.Remove(document);

        // Apply changes to the database.
        return _dbContext.SaveChangesAsync();
    }
    #endregion

    #region Delete order
    /// <summary>
    /// Delete an element of table TCMD_COMMANDES.
    /// It is only possible for an order in phase draft.
    /// </summary>
    /// <param name="id">Id of the order (TCMD_COMMANDEID).</param>
    /// <param name="userId">Id of the user who requested the delete.</param>
    /// <returns>Http status code.</returns>
    public async Task<IActionResult> DeleteOrderAsync(int id, string userId)
    {
        try
        {
            // Check id value.
            if (id <= 0)
                return new StatusCodeResult(StatusCodes.Status400BadRequest);

            // Get order.
            var order = _dbContext.TCMD_COMMANDES
                .Include(cmd => cmd.TCMD_PH_PHASE)
                .FirstOrDefault(cmd => cmd.TCMD_COMMANDEID.Equals(id));

            // Verify if order exists.
            if (order is null)
                // Code 410: Target ressource is no longer available.
                return new StatusCodeResult(StatusCodes.Status410Gone);

            // Get draft phase ID.
            int? draftPhaseId = _dbContext.TCMD_PH_PHASES
                .FirstOrDefault(ph => ph.TCMD_PH_CODE.Equals("BR"))?
                .TCMD_PH_PHASEID;

            // Verify that the order is in draft phase.
            if (draftPhaseId is null
                || !draftPhaseId.Equals(order.TCMD_PH_PHASE?.TCMD_PH_PHASEID))
                return new StatusCodeResult(StatusCodes.Status409Conflict);

            // Get list of joined documents.
            var documents = _dbContext.TCMD_DOC_DOCUMENTS
                .Where(doc => doc.TCMD_COMMANDEID.Equals(id));

            // Delete joined documents.
            foreach (var document in documents)
            {
                await DeleteDocumentAsync(document);
            }

            // Delete order => Delete entry in TCMD_COMMANDES.
            _dbContext.Remove(order);

            // Apply changes to the database.
            await _dbContext.SaveChangesAsync();

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[] { typeof(TCMD_COMMANDES).FullName },
                Litterals.Delete,
                userId,
                nameof(DeleteOrderAsync));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(OrdersServices)}.{nameof(DeleteOrderAsync)}: {ex.Message}",
                ex.InnerException?.Message);

            // Code 500: Internal server error.
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        // Code 204: Action performed and the response does not include any content.
        return new StatusCodeResult(StatusCodes.Status204NoContent);
    }
    #endregion

    #region Order phase transition methods and class
    /// <summary>
    /// Describes the phase transitions of an order.
    /// </summary>
    private class OrderPhaseTransition
    {
        // Phase before transition.
        public readonly string PhaseCodeBefore;

        // Phase after transition.
        public readonly string PhaseCodeAfter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="phaseCodeBefore">Phase before transition.</param>
        /// <param name="phaseCodeAfter">Phase after transition.</param>
        public OrderPhaseTransition(string phaseCodeBefore, string phaseCodeAfter)
        {
            PhaseCodeBefore = phaseCodeBefore;
            PhaseCodeAfter = phaseCodeAfter;
        }
    }

    /// <summary>
    /// List allowed transitions.
    /// </summary>
    private readonly IEnumerable<OrderPhaseTransition> _transitions = new List<OrderPhaseTransition>
    {
        // From "Draft" phase:
        new(Phases.Draft, Phases.ToAccept),
        new(Phases.Draft, Phases.Archived),
        // From "To accept":
        new(Phases.ToAccept, Phases.InProgress),
        new(Phases.ToAccept, Phases.Rejected),
        new(Phases.ToAccept, Phases.Frost),
        new(Phases.ToAccept, Phases.Canceled),
        new(Phases.ToAccept, Phases.Archived),
        // From "In progress":
        new(Phases.InProgress, Phases.Rejected),
        new(Phases.InProgress, Phases.Frost),
        new(Phases.InProgress, Phases.Delivered),
        new(Phases.InProgress, Phases.Canceled),
        new(Phases.InProgress, Phases.Archived),
        // From "Rejected":
        new(Phases.Rejected, Phases.Archived),
        // From "Frost":
        new(Phases.Frost, Phases.InProgress),
        new(Phases.Frost, Phases.Rejected),
        new(Phases.Frost, Phases.Delivered),
        new(Phases.Frost, Phases.Canceled),
        new(Phases.Frost, Phases.Archived),
        // From "Delivered":
        new(Phases.Delivered, Phases.Completed),
        new(Phases.Delivered, Phases.Canceled),
        new(Phases.Delivered, Phases.Archived),
        // From "Completed":
        new(Phases.Completed, Phases.Archived),
        // From "Canceled":
        new(Phases.Canceled, Phases.Archived),
        // From "Archived":
        // No transition allowed.
    };

    /// <summary>
    /// Control that a transition between phases is possible.
    /// </summary>
    /// <param name="phaseCodeBefore">Phase before transition.</param>
    /// <param name="phaseCodeAfter">Phase after transition.</param>
    /// <returns>True if transition is allowed, false otherwise.</returns>
    private bool IsPhaseTransitionAllowed(string phaseCodeBefore, string phaseCodeAfter)
        => _transitions.Any(t => t.PhaseCodeBefore.Equals(phaseCodeBefore, StringComparison.Ordinal)
                            && t.PhaseCodeAfter.Equals(phaseCodeAfter, StringComparison.Ordinal));
    #endregion

    #region Change order phase
    /// <summary>
    /// Change the phase of an element of table TCMD_COMMANDES.
    /// </summary>
    /// <param name="id">Id of the order (TCMD_COMMANDEID).</param>
    /// <param name="userId">Id of the user requesting the phase change.</param>
    /// <param name="args">Change order phase arguments.</param>
    /// <returns>Http status code.</returns>
    public async Task<IActionResult> ChangeOrderPhaseAsync(int id, string userId, ChangeOrderPhaseArguments args)
    {
        // Check Id value.
        if (id <= 0)
            // Bad parameter.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Get order.
        var order = _dbContext.TCMD_COMMANDES
            .Include(cmd => cmd.TCMD_PH_PHASE)
            .FirstOrDefault(cmd => cmd.TCMD_COMMANDEID.Equals(id));

        // Verify if order exists.
        if (order is null)
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        // Check arguments.
        if (args is null ||
            string.IsNullOrEmpty(args.PhaseCode) ||
            args.ReasonId.Equals(default))
        {
            // Bad parameters.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        // Get arguments phase ID.
        int? phaseId = _dbContext.TCMD_PH_PHASES
            .FirstOrDefault(ph => ph.TCMD_PH_CODE.Equals(args.PhaseCode))?
            .TCMD_PH_PHASEID;

        // Verify if phase exists.
        if (phaseId is null)
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Get arguments reason.
        var raisonPhases = _dbContext.TCMD_RP_RAISON_PHASES
            .FirstOrDefault(rp => rp.TCMD_RP_RAISON_PHASEID.Equals(args.ReasonId)
            && phaseId.Equals(rp.TCMD_PH_PHASEID));

        // Verify if phase reason exists.
        if (raisonPhases is null)
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Verify that the phase change is allowed.
        if (!IsPhaseTransitionAllowed(order.TCMD_PH_PHASE.TCMD_PH_CODE, args.PhaseCode))
            return new StatusCodeResult(StatusCodes.Status409Conflict);

        // Start a transaction.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Define a new entry for the phase change history.
            TCMD_SP_SUIVI_PHASES suiviPhases = new()
            {
                TCMD_COMMANDEID = order.TCMD_COMMANDEID,
                TCMD_SP_DATE_MODIF = DateExtensions.GetUtcNowSecond(),
                TRU_AUTEUR_MODIFID = userId,
                TCMD_SP_COMMENTAIRE = args.Comment,
                TCMD_PH_PHASE_AVANTID = order.TCMD_PH_PHASEID,
                TCMD_PH_PHASE_APRESID = phaseId.Value
            };

            // Add phase change history in table.
            _dbContext.TCMD_SP_SUIVI_PHASES.Add(suiviPhases);

            // Apply changes to the database.
            await _dbContext.SaveChangesAsync();

            // Define reason for the phase change.
            TCMD_CR_CMD_RAISON_PHASES cmdRaisonPhases = new()
            {
                TCMD_SP_SUIVI_PHASEID = suiviPhases.TCMD_SP_SUIVI_PHASEID,
                TCMD_RP_RAISON_PHASEID = args.ReasonId
            };

            // Add phase change reason in table.
            _dbContext.TCMD_CR_CMD_RAISON_PHASES.Add(cmdRaisonPhases);

            // Update order entry in table.
            order.TCMD_PH_PHASEID = phaseId.Value;
            order.TCMD_DATE_MODIF = DateExtensions.GetUtcNowSecond();
            // Add date on which the order is transmitted (to the operator).
            if (Phases.ToAccept.Equals(args.PhaseCode))
                order.TCMD_DATE_PASSAGE_CMD = DateExtensions.GetUtcNowSecond();
            // Add date on which the order begins to be processed by the operator.
            if (Phases.InProgress.Equals(args.PhaseCode) && order.TCMD_DATE_PRISE_EN_CHARGE is null)
                order.TCMD_DATE_PRISE_EN_CHARGE = DateExtensions.GetUtcNowSecond();
            // Add delivery date.
            if (Phases.Delivered.Equals(args.PhaseCode))
                order.TCMD_DATE_LIVRAISON = DateExtensions.GetUtcNowSecond();
            // Add date on which the order is closed.
            if (Phases.Rejected.Equals(args.PhaseCode) || Phases.Completed.Equals(args.PhaseCode))
                order.TCMD_DATE_CLOTURE = DateExtensions.GetUtcNowSecond();

            // Apply changes to the database.
            await _dbContext.SaveChangesAsync();

            // Commit transaction.
            // Transaction will auto-rollback when disposed if either commands fails.
            await transaction.CommitAsync();

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[] {
                    typeof(TCMD_COMMANDES).FullName,
                    typeof(TCMD_SP_SUIVI_PHASES).FullName
                },
                Litterals.Update,
                userId,
                nameof(ChangeOrderPhaseAsync));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(OrdersServices)}.{nameof(ChangeOrderPhaseAsync)}: {ex.Message}",
                ex.InnerException?.Message);

            // Code 500: Internal server error.
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new StatusCodeResult(StatusCodes.Status200OK);
    }
    #endregion

    #region Duplicate order
    /// <summary>
    /// Duplicate an element of table TCMD_COMMANDES.
    /// Duplicates the associated documents too.
    /// </summary>
    /// <param name="id">Id of the order to duplicate.</param>
    /// <param name="userId">Id of the user requesting the duplication.</param>
    /// <returns>HTTP response (with the given response status code).</returns>
    public async Task<IActionResult> DuplicateOrderAsync(int id, string userId)
    {
        // Check Ids.
        if (id <= 0 || userId is null)
            // Bad parameter.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Get draft phase ID.
        int? draftPhaseId = _dbContext.TCMD_PH_PHASES
            .FirstOrDefault(ph => ph.TCMD_PH_CODE.Equals(Phases.Draft))?
            .TCMD_PH_PHASEID;

        // Get copy creation mode ID.
        int? copyCreationModeId = _dbContext.TCMD_MC_MODE_CREATIONS
            .FirstOrDefault(mc => mc.TCMD_MC_CODE.Equals(CreationModes.Copy))?
            .TCMD_MC_MODE_CREATIONID;

        // Verify draft phase ID and copy creation mode ID.
        if (draftPhaseId is null || copyCreationModeId is null)
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);

        // Get order and joined documents.
        // Do not track because the entity will be duplicated.
        var order = _dbContext.TCMD_COMMANDES
            .Include(cmd => cmd.TCMD_DOC_DOCUMENTS)
            .Include(cmd => cmd.TCMD_PH_PHASE)
            .AsNoTracking()
            .FirstOrDefault(cmd => cmd.TCMD_COMMANDEID.Equals(id));

        // Verify if order exists.
        if (order is null)
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        // Verify that the order is in a phase that allow duplication.
        if (Phases.Draft.Equals(order.TCMD_PH_PHASE?.TCMD_PH_CODE))
            return new StatusCodeResult(StatusCodes.Status409Conflict);

        try
        {
            // Start a transaction.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Create new order.
            TCMD_COMMANDES newOrder = new()
            {
                TCMD_DATE_CREATION = DateExtensions.GetUtcNowSecond(),
                TCMD_DATE_MODIF = DateExtensions.GetUtcNowSecond(),
                TRU_COMMANDITAIREID = userId,
                TCMD_COMMENTAIRE = order.TCMD_COMMENTAIRE,
                TCMD_PH_PHASEID = (int)draftPhaseId,
                TCMD_MC_MODE_CREATIONID = (int)copyCreationModeId,
                TDOM_DOMAINEID = order.TDOM_DOMAINEID,
                TE_ETATID = order.TE_ETATID,
                TS_SCENARIOID = order.TS_SCENARIOID,
                TCMD_ORIGINEID = order.TCMD_COMMANDEID,
                TCMD_DOC_DOCUMENTS = order.TCMD_DOC_DOCUMENTS
            };

            // Reset documents IDs because documents are new entries in the base.
            // NewOrder.TCMD_DOC_DOCUMENTS.TCMD_COMMANDEID is updated automatically (by EF).
            foreach (var document in newOrder.TCMD_DOC_DOCUMENTS)
            {
                document.TCMD_DOC_DOCUMENTID = default;
            }

            // Add duplicated order.
            await _dbContext.TCMD_COMMANDES.AddAsync(newOrder);

            // Apply changes to the database.
            await _dbContext.SaveChangesAsync();

            // Duplicate documents (files).
            foreach (var document in newOrder.TCMD_DOC_DOCUMENTS)
            {
                // Get source file path.
                string sourceFilePath = GetDocumentPath(order.TCMD_COMMANDEID,
                    document.TCMD_DOC_FILENAME);

                // Get destination file path.
                string destinationFilePath = GetDocumentPath(newOrder.TCMD_COMMANDEID,
                    document.TCMD_DOC_FILENAME);

                // Create destination directory (if it does not already exist).
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);

                // Copy file (overwriting is allowed).
                File.Copy(sourceFilePath, destinationFilePath, true);
            }

            // Commit transaction.
            // Transaction will auto-rollback when disposed if either commands fails.
            await transaction.CommitAsync();

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[]
                {
                    typeof(TCMD_COMMANDES).FullName,
                    typeof(TCMD_DOC_DOCUMENTS).FullName
                },
                Litterals.Insert,
                userId,
                nameof(DuplicateOrderAsync));

            return new CreatedResult($"/{nameof(TCMD_COMMANDES)}/{newOrder.TCMD_COMMANDEID}",
                newOrder);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(OrdersServices)}.{nameof(SaveDocumentAsync)}: {ex.Message}", ex.InnerException?.Message);

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
    #endregion

    #region Upload document
    /// <summary>
    /// Save a document (in a directory and in database).
    /// </summary>
    /// <param name="uploadFiles">Uploaded document.</param>
    /// <param name="orderId">Id of the order (TCMD_COMMANDEID).</param>
    /// <param name="userId">Id of the user adding a document.</param>
    /// <returns>Http status code.</returns>
    public async Task<IActionResult> SaveDocumentAsync(IList<IFormFile> uploadFiles, int orderId, string userId)
    {
        // Check ids.
        if (orderId <= 0 || userId is null)
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Check there is an uploaded file.
        if (!uploadFiles.Any())
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Verify if order exists.
        if (!_dbContext.TCMD_COMMANDES.Any(cmd => cmd.TCMD_COMMANDEID.Equals(orderId)))
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        // Get file. (Documents are uploaded one by one.)
        var file = uploadFiles.FirstOrDefault();

        // Define a new entry for the document table.
        TCMD_DOC_DOCUMENTS document = new()
        {
            TCMD_DOC_FILENAME = file?.FileName,
            TCMD_DOC_DATE = DateExtensions.GetUtcNowSecond(),
            TCMD_DOC_COMMENTAIRE = string.Empty,
            TCMD_COMMANDEID = orderId,
            TCMD_DOC_TAILLE = (int?)file?.Length
        };

        // Start a transaction.
        // Here we manually control the transaction instead of relying on the default behavior
        // because we want to apply the changes on database after the files have been written to disk. 
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Add entry in table.
            _dbContext.TCMD_DOC_DOCUMENTS.Add(document);

            // Apply changes to the database.
            await _dbContext.SaveChangesAsync();

            // Get file path.
            string filePath = GetDocumentPath(orderId, file?.FileName);

            // Create directory.
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            // Create file at specified path.
            await using (var stream = File.Create(filePath))
            {
                if (file != null)
                {
                    await file.CopyToAsync(stream);
                }
            }

            // Commit transaction.
            await transaction.CommitAsync();

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[]
                {
                    typeof(TCMD_COMMANDES).FullName,
                    typeof(TCMD_DOC_DOCUMENTS).FullName
                },
                Litterals.Insert,
                userId,
                nameof(SaveDocumentAsync));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(OrdersServices)}.{nameof(SaveDocumentAsync)}: {ex.Message}", ex.InnerException?.Message);

            try
            {
                transaction.Rollback();
            }
            catch (Exception)
            {
                // Catch errors that may have occurred on the server
                // and that would cause the rollback to fail, such as
                // a closed connection.
            }

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new StatusCodeResult(StatusCodes.Status200OK);
    }
    #endregion

    #region Remove document
    /// <summary>
    /// Remove a document (from directory and database).
    /// </summary>
    /// <param name="documentName">Name of the document to remove.</param>
    /// <param name="orderId">Id of the order (TCMD_COMMANDEID).</param>
    /// <param name="userId">Id of the user removing a document.</param>
    /// <returns>Http status code.</returns>
    public async Task<IActionResult> RemoveDocumentAsync(string documentName, int orderId, string userId)
    {
        // Check ids.
        if (orderId <= 0 || userId is null)
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Check there is a file.
        if (documentName is null)
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Get order.
        var order = _dbContext.TCMD_COMMANDES
            .Include(cmd => cmd.TCMD_PH_PHASE)
            .Include(cmd => cmd.TCMD_DOC_DOCUMENTS)
            .FirstOrDefault(cmd => cmd.TCMD_COMMANDEID.Equals(orderId));

        // Verify if order exists.
        if (order is null)
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        // Get draft phase ID.
        int? draftPhaseId = _dbContext.TCMD_PH_PHASES
            .FirstOrDefault(ph => ph.TCMD_PH_CODE.Equals(Phases.Draft))?
            .TCMD_PH_PHASEID;

        // Verify that the order is in draft phase.
        if (draftPhaseId is null
            || !draftPhaseId.Equals(order.TCMD_PH_PHASE?.TCMD_PH_PHASEID))
            return new StatusCodeResult(StatusCodes.Status409Conflict);

        // Get document entry in database.
        var document = order.TCMD_DOC_DOCUMENTS
            .FirstOrDefault(doc => doc.TCMD_DOC_FILENAME.Equals(documentName));

        // Verify if document exists.
        if (document is null)
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        try
        {
            await DeleteDocumentAsync(document);

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[]
                {
                    typeof(TCMD_COMMANDES).FullName,
                    typeof(TCMD_DOC_DOCUMENTS).FullName
                },
                Litterals.Delete,
                userId,
                nameof(RemoveDocumentAsync));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(OrdersServices)}.{nameof(RemoveDocumentAsync)}: {ex.Message}", ex.InnerException?.Message);

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new StatusCodeResult(StatusCodes.Status200OK);
    }
    #endregion

    #region Read associated Productions
    /// <summary>
    /// Read productions associated with the selected order.
    /// </summary>
    /// <param name="orderId">Id of the selected order.</param>
    /// <returns>HTTP response (with the given response status code).</returns>
    public IActionResult ReadAssociatedProductions(int orderId)
    {
        // Check Id.
        if (orderId <= 0)
            // Bad parameter.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Read view of extended productions (VDE_DEMANDES_ETENDUES)
        // which are associated with selected orders through the TCMD_DA_DEMANDES_ASSOCIEES table.
        // Orders should be notables.
        var extendedProductions =
            from demandesAssociees in _dbContext.TCMD_DA_DEMANDES_ASSOCIEES
            join demandesEtendues in _dbContext.VDE_DEMANDES_ETENDUES on demandesAssociees.TD_DEMANDEID equals demandesEtendues.TD_DEMANDEID
            where demandesAssociees.TCMD_COMMANDEID.Equals(orderId)
                && demandesAssociees.TCMD_DA_VERSION_NOTABLE.Equals(1) // true
            select demandesEtendues;

        return new OkObjectResult(extendedProductions);
    }
    #endregion
}
