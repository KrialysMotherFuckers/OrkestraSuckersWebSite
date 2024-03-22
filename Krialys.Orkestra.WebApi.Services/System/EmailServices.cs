using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Data.EF.Resources;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models.Email;
using Krialys.Orkestra.WebApi.Services.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using System.Globalization;
using System.Text;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Services.System;

public interface IEmailServices : IScopedService
{
    Task<IEnumerable<TR_MEL_EMail_Templates>> GetEmailTemplateList();

    /// <summary>
    /// Build ad'hoc HTML (including CSS) then send mail to DEMANDEUR using his/her language preference
    /// </summary>
    /// <param name="demandeId">TD_DEMANDEID</param>
    /// <param name="typeId">Mail type (0 == Production, 1 == Commande)</param>
    /// <returns>Tuple made of: Success scenarii flag, Error message if any</returns>
    Task<bool> SendAutomatedMailForRequest(int demandeId, string typeId);

    /// <summary>
    /// Sends the automated mail for orders.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="statusTypeCode">The status type code.</param>
    /// <returns></returns>
    Task<bool> SendAutomatedMailForOrder(int orderId, string statusTypeCode);

    /// <summary>
    /// Send generic email using EmailTemplate class
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    Task<bool> SendGenericMail(EmailTemplate template);
}

public class EmailServices : IEmailServices
{
    #region CTOR

    private class EmailRequest
    {
        public TR_MEL_EMail_Templates Template { get; set; }

        public TD_DEMANDES Demande { get; set; }

        public VDE_DEMANDES_ETENDUES DemandeEtendue { get; set; }

        public TRU_USERS User { get; set; }

        public int DemandeId { get; set; }

        public string TypeCode { get; set; }

        public string Error { get; set; }

        private (string Object, string Body, string Footer) _content;

        public (string Object, string Body, string Footer) Content
        {
            get
            {
                if (_content.Object is not null && _content.Body is not null && _content.Footer is not null)
                    return _content;

                try
                {
                    if (Template != null && User != null)
                    {
                        if (string.IsNullOrEmpty(User.TRU_EMAIL))
                        {
                            Error = $"{User.TRU_FULLNAME} has no e-mail, please contact your administrator.";
                        }
                        else
                        {
                            // Get root language
                            var lng = User.TRLG_LNGID.Substring(0, 2).ToUpper();
                            var status = lng == CultureLiterals.French ? DemandeEtendue.STATUT_DEMANDE_FR : DemandeEtendue.STATUT_DEMANDE_EN;

                            // Object
                            string obj = Template.mel_email_subject
                                .Replace("[TD_DEMANDEID]", DemandeId.ToString())
                                .Replace($"[STATUT_DEMANDE_{lng}]", status);

                            // Body + replace HtmlQualifsTable within SendAutomatedMail function call
                            string body = Template.mel_email_body
                                .Replace("[TE_NOM_ETAT_VERSION]", DemandeEtendue.TE_NOM_ETAT_VERSION)
                                .Replace("[TS_NOM_SCENARIO]", DemandeEtendue.TS_NOM_SCENARIO)
                                .Replace($"[STATUT_DEMANDE_{lng}]", status)
                                .Replace("[TD_DUREE_PRODUCTION_REEL]",
                                    DemandeEtendue.TD_DUREE_PRODUCTION_REEL.HasValue
                                        ? $"{TimeSpan.FromSeconds(DemandeEtendue.TD_DUREE_PRODUCTION_REEL.Value):hh\\:mm\\:ss} (hour:min:sec)."
                                        : "-");
                            // Footer
                            string footer = Template.mel_email_footer
                                .Replace("[LOGO_ORKESTRA]", $"<div><img src='{CssLiterals.LogoOrkestra}' alt='Logo OrKestra'></div>");

                            Error = null;

                            _content = (obj, body, footer);

                            return _content;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error = $"Translating content error: {ex.Message}";
                }

                _content = (Object: null, Body: null, Footer: null);

                return _content;
            }
        }
    }

    private class EmailOrder
    {
        public TR_MEL_EMail_Templates Template { get; set; }

        public TCMD_COMMANDES Order { get; set; }

        public TCMD_SP_SUIVI_PHASES TCMD_SP_SUIVI_PHASES { get; set; }

        public TRU_USERS User { get; set; }

        public int OrderId { get; set; }

        public string TypeCode { get; set; }

        public string Error { get; set; }

        private (string Object, string Body, string Footer) _content;

        public (string Object, string Body, string Footer) Content
        {
            get
            {
                if (_content.Object is not null && _content.Body is not null && _content.Footer is not null)
                    return _content;

                try
                {
                    if (Template != null && User != null)
                    {
                        if (string.IsNullOrEmpty(User.TRU_EMAIL))
                        {
                            Error = $"{User.TRU_FULLNAME} has no e-mail, please contact your administrator.";
                        }
                        else
                        {
                            // Object
                            string obj = Template.mel_email_subject
                                .Replace("[TCMD_COMMANDEID]", Order.TCMD_COMMANDEID.ToString());

                            // Body + replace HtmlQualifsTable within SendAutomatedMail function call
                            string body = Template.mel_email_body
                                .Replace("[TCMD_COMMANDEID]", Order.TCMD_COMMANDEID.ToString())

                                .Replace("[TMCD_EXPLOITANTID]", Order.TRU_EXPLOITANT?.TRU_FULLNAME)
                                
                                .Replace("[TRU_AUTEUR_MODIFID]", TCMD_SP_SUIVI_PHASES?.TRU_AUTEUR_MODIF?.TRU_FULLNAME)
                                .Replace("[TMCD_SP_DATE_MODIF]", TCMD_SP_SUIVI_PHASES?.TCMD_SP_DATE_MODIF?.ToString())

                                .Replace("[TMCD_RP_LIB_FR]", Order.TCMD_PH_PHASE?.TCMD_PH_LIB_FR)
                                .Replace("[TMCD_RP_LIB_EN]", Order.TCMD_PH_PHASE?.TCMD_PH_LIB_EN)

                                .Replace("[TMCD_SP_COMMENTAIRE]", TCMD_SP_SUIVI_PHASES?.TCMD_SP_COMMENTAIRE);

                            // Footer
                            string footer = Template.mel_email_footer
                                .Replace("[LOGO_ORKESTRA]", $"<div><img src='{CssLiterals.LogoOrkestra}' alt='Logo OrKestra'></div>");

                            Error = null;

                            _content = (obj, body, footer);

                            return _content;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error = $"Translating content error: {ex.Message}";
                }

                _content = (Object: null, Body: null, Footer: null);

                return _content;
            }
        }
    }

    private readonly KrialysDbContext _dbUniversContext;
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbEtqContext;
    private readonly IDataAnnotations _dataAnnotations;
    private readonly ILanguageContainerService _trad;
    private readonly ICommonServices _commonServices;
    private readonly IConfiguration _configuration;

    public EmailServices(
        KrialysDbContext dbUniversContext,
        Krialys.Data.EF.Etq.KrialysDbContext dbEtqContext,
        IConfiguration configuration,
        IDataAnnotations dataAnnotations,
        ILanguageContainerService trad,
        ICommonServices commonServices)
    {
        _dbUniversContext = dbUniversContext ?? throw new ArgumentNullException(nameof(dbUniversContext));
        _dbEtqContext = dbEtqContext ?? throw new ArgumentNullException(nameof(dbEtqContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dataAnnotations = dataAnnotations ?? throw new ArgumentNullException(nameof(dataAnnotations));
        _commonServices = commonServices;

        _trad = trad;
    }
    #endregion

    public async Task<IEnumerable<TR_MEL_EMail_Templates>> GetEmailTemplateList() => await _dbUniversContext.TR_MEL_EMail_Templates.ToListAsync();


    /// <summary>
    /// Build ad'hoc HTML (including CSS) then send mail to DEMANDEUR using his/her language preference
    /// </summary>
    /// <param name="demandeId">TD_DEMANDEID</param>
    /// <param name="typeCode">Mail type (0 == Production, 1 == Commande)</param>
    /// <returns>Tuple made of: Success scenarii flag, Error message if any</returns>
    public async Task<bool> SendAutomatedMailForRequest(int demandeId, string typeCode)
    {
        EmailTemplate template = null;
        EmailRequest _eMail = new();

        try
        {
            // SMTP and SecureSocketOptions parameters => if NO SMTP then no mail is sent!
            var host = _configuration["MailKit:SMTP:Host"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrWhiteSpace(host)) return false;

            var port = _configuration.GetValue<int>("MailKit:SMTP:Port");
            var user = _configuration["MailKit:SMTP:User"];
            var pass = _configuration["MailKit:SMTP:Pass"];
            var sock = (SecureSocketOptions)_configuration.GetValue<int>("MailKit:SMTP:SecureSocketOptions");

            // Demande Id
            _eMail.DemandeId = demandeId;

            // Mail type
            _eMail.TypeCode = typeCode;

            // Demande
            _eMail.Demande = _dbUniversContext.TD_DEMANDES.FirstOrDefault(e => e.TD_DEMANDEID == demandeId);

            // Demande étendue
            _eMail.DemandeEtendue = _dbUniversContext.VDE_DEMANDES_ETENDUES.FirstOrDefault(e => e.TD_DEMANDEID == demandeId);

            // User
            if (_eMail.Demande != null && !string.IsNullOrEmpty(_eMail.Demande.TRU_DEMANDEURID))
                _eMail.User = _dbUniversContext.TRU_USERS.FirstOrDefault(e => e.TRU_STATUS == StatusLiteral.Available
                    && e.TRU_USERID == _eMail.Demande.TRU_DEMANDEURID);

            // User's Culture
            if (_eMail.User != null && string.IsNullOrEmpty(_eMail.User.TRLG_LNGID))
                _eMail.User.TRLG_LNGID = _configuration["CultureInfo"];

            // Force Culture (mandatory if you want to have translated items at runtime)
            if (_eMail.User?.TRLG_LNGID != null)
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(_eMail.User.TRLG_LNGID);
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture;
                _trad.SetLanguage(Thread.CurrentThread.CurrentCulture);

                // HTML Template
                _eMail.Template = GetMailTemplate(typeCode, _eMail.User.TRLG_LNGID);

                // Check body content
                if (_eMail.Content.Body is null)
                    throw new Exception(_eMail.Error);

                var emailMessage = new MimeMessage();
                if (_eMail.Template is not null)
                {
                    // Check again if no error before sending mail
                    if (!string.IsNullOrEmpty(_eMail.Error))
                        throw new Exception(_eMail.Error);

                    // Email body including Qualifs
                    var body = _eMail.Content.Body.Contains("[TABLEAU_FEUX_QUALIF]")
                        ? _eMail.Content.Body.Replace("[TABLEAU_FEUX_QUALIF]", HtmlQualifsTable(demandeId))
                        : _eMail.Content.Body;

                    // Build HTML
                    var html = $"{HtmlHeader}{body}{_eMail.Content.Footer}{HtmlFooter}";

                    // _eMail.Template.TMT_IMPORTANCE = "H"; /* => TEST OK */

                    // Use generic template
                    template = new EmailTemplate
                    {
                        From = $"Orkestra Noreply <{_configuration["MailKit:SMTP:Sender"]}>",
                        To = _eMail.User.TRU_EMAIL,
                        Subject = $"[{_configuration["Environment"]}] {_eMail.Content.Object}",
                        Importance = _eMail.Template.mel_email_importance switch
                        {
                            "H" => Importance.High,
                            "N" => Importance.Normal,
                            "L" => Importance.Low,
                            _ => Importance.Low,
                        },
                        Priority = _eMail.Template.mel_email_importance switch
                        {
                            "H" => Priority.High,
                            "N" => Priority.Normal,
                            "L" => Priority.Low,
                            _ => Priority.Low,
                        },
                        TextFormatHtml = true,
                        Body = html,
                        CultureInfo = _eMail.User.TRLG_LNGID,
                        TimeZone = _eMail.User.TRTZ_TZID
                    };

                    // Create message and set properties
                    emailMessage.From.Add(MailboxAddress.Parse(template.From));

                    foreach (var to in template.To.Split(';'))
                    {
                        to.Trim().With(address =>
                        {
                            if (address.Length > 0)
                                emailMessage.To.Add(MailboxAddress.Parse(address));
                        });
                    }

                    emailMessage.Subject = template.Subject;
                    emailMessage.Body = new TextPart(template.TextFormatHtml ? TextFormat.Html : TextFormat.Text) { Text = template.Body };
                    emailMessage.Importance = (MessageImportance)template.Importance;
                    emailMessage.XPriority = (XMessagePriority)template.Priority;
                    emailMessage.MessageId = MimeUtils.GenerateMessageId();

                    // Send email via SMTP
                    using var client = new SmtpClient();

                    await client.ConnectAsync(host, port, sock);
                    await client.AuthenticateAsync(user, pass);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);

                    _eMail.Error = null;

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            var user = _commonServices.GetUserIdAndName();
            _commonServices.StdLogException(new LogException(GetType(), ex), user.userId, user.userName,
                new Dictionary<string, object> {
                    { nameof(template.Subject), template?.Subject },
                    { nameof(template.From), template?.From },
                    { nameof(template.To), template?.To },
                    { nameof(template.TimeZone), template?.TimeZone },
                    { nameof(template.CultureInfo), template?.CultureInfo }
                });
        }

        return false;
    }


    /// <summary>
    /// Sends the automated mail for orders.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="statusTypeCode">The status type code.</param>
    /// <returns></returns>
    public async Task<bool> SendAutomatedMailForOrder(int orderId, string statusTypeCode)
    {
        EmailTemplate template = null;
        EmailOrder _eMail = new();

        try
        {
            // SMTP and SecureSocketOptions parameters => if NO SMTP then no mail is sent!
            var host = _configuration["MailKit:SMTP:Host"];
            if (string.IsNullOrEmpty(host) || string.IsNullOrWhiteSpace(host)) return false;

            var port = _configuration.GetValue<int>("MailKit:SMTP:Port");
            var user = _configuration["MailKit:SMTP:User"];
            var pass = _configuration["MailKit:SMTP:Pass"];
            var sock = (SecureSocketOptions)_configuration.GetValue<int>("MailKit:SMTP:SecureSocketOptions");

            _eMail.OrderId = orderId;
            _eMail.TypeCode = statusTypeCode;
            _eMail.Order = _dbUniversContext.TCMD_COMMANDES.FirstOrDefault(e => e.TCMD_COMMANDEID == orderId);

            _eMail.TCMD_SP_SUIVI_PHASES = _dbUniversContext.TCMD_SP_SUIVI_PHASES.FirstOrDefault(x => x.TCMD_COMMANDEID == orderId && x.TCMD_PH_PHASE_APRESID == _eMail.Order.TCMD_PH_PHASEID);

            // User
            if (_eMail.Order != null && !string.IsNullOrEmpty(_eMail.Order.TRU_COMMANDITAIREID))
                _eMail.User = _dbUniversContext.TRU_USERS.FirstOrDefault(e => e.TRU_STATUS == StatusLiteral.Available
                    && e.TRU_USERID == _eMail.Order.TRU_COMMANDITAIREID);

            // User's Culture
            if (_eMail.User != null && string.IsNullOrEmpty(_eMail.User.TRLG_LNGID))
                _eMail.User.TRLG_LNGID = _configuration["CultureInfo"];

            // Force Culture (mandatory if you want to have translated items at runtime)
            if (_eMail.User?.TRLG_LNGID != null)
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(_eMail.User.TRLG_LNGID);
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture;
                _trad.SetLanguage(Thread.CurrentThread.CurrentCulture);

                // HTML Template
                _eMail.Template = GetMailTemplate(statusTypeCode, _eMail.User.TRLG_LNGID);

                // Check body content
                if (_eMail.Content.Body is null)
                    throw new Exception(_eMail.Error);

                var emailMessage = new MimeMessage();
                if (_eMail.Template is not null)
                {
                    // Check again if no error before sending mail
                    if (!string.IsNullOrEmpty(_eMail.Error)) throw new Exception(_eMail.Error);

                    var body = _eMail.Content.Body;

                    // Build HTML
                    var html = $"{HtmlHeader}{body}{_eMail.Content.Footer}{HtmlFooter}";

                    // _eMail.Template.TMT_IMPORTANCE = "H"; /* => TEST OK */

                    // Use generic template
                    template = new EmailTemplate
                    {
                        From = $"Orkestra Noreply <{_configuration["MailKit:SMTP:Sender"]}>",
                        To = _eMail.User.TRU_EMAIL,
                        Subject = $"[{_configuration["Environment"]}] {_eMail.Content.Object}",
                        Importance = _eMail.Template.mel_email_importance switch
                        {
                            "H" => Importance.High,
                            "N" => Importance.Normal,
                            "L" => Importance.Low,
                            _ => Importance.Low,
                        },
                        Priority = _eMail.Template.mel_email_importance switch
                        {
                            "H" => Priority.High,
                            "N" => Priority.Normal,
                            "L" => Priority.Low,
                            _ => Priority.Low,
                        },
                        TextFormatHtml = true,
                        Body = html,
                        CultureInfo = _eMail.User.TRLG_LNGID,
                        TimeZone = _eMail.User.TRTZ_TZID
                    };

                    // Create message and set properties
                    emailMessage.From.Add(MailboxAddress.Parse(template.From));

                    foreach (var to in template.To.Split(';'))
                    {
                        to.Trim().With(address =>
                        {
                            if (address.Length > 0)
                                emailMessage.To.Add(MailboxAddress.Parse(address));
                        });
                    }

                    emailMessage.Subject = template.Subject;
                    emailMessage.Body = new TextPart(template.TextFormatHtml ? TextFormat.Html : TextFormat.Text) { Text = template.Body };
                    emailMessage.Importance = (MessageImportance)template.Importance;
                    emailMessage.XPriority = (XMessagePriority)template.Priority;
                    emailMessage.MessageId = MimeUtils.GenerateMessageId();

                    // Send email via SMTP
                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync(host, port, sock);
                        await client.AuthenticateAsync(user, pass);
                        await client.SendAsync(emailMessage);
                        await client.DisconnectAsync(true);
                    }

                    _eMail.Error = null;

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            var user = _commonServices.GetUserIdAndName();
            _commonServices.StdLogException(new LogException(GetType(), ex), user.userId, user.userName,
                new Dictionary<string, object> {
                    { nameof(template.Subject), template?.Subject },
                    { nameof(template.From), template?.From },
                    { nameof(template.To), template?.To },
                    { nameof(template.TimeZone), template?.TimeZone },
                    { nameof(template.CultureInfo), template?.CultureInfo }
                });
        }

        return false;
    }
    /// <summary>
    /// Send generic email using EmailTemplate class
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    public async Task<bool> SendGenericMail(EmailTemplate template)
    {
        var email = new MimeMessage();
        try
        {
            // SMTP and SecureSocketOptions parameters => if NO SMTP then no mail is sent!
            var host = _configuration["MailKit:SMTP:Host"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            var port = _configuration.GetValue<int>("MailKit:SMTP:Port");
            var user = _configuration["MailKit:SMTP:User"];
            var pass = _configuration["MailKit:SMTP:Pass"];
            var sock = (SecureSocketOptions)_configuration.GetValue<int>("MailKit:SMTP:SecureSocketOptions");

            // Identified cultureInfo and timeZone
            (template.CultureInfo, template.TimeZone) = _commonServices.GetUserCultureInfoAndTimeZone();

            // Create message
            email.From.Add(MailboxAddress.Parse(template.From));

            foreach (var to in template.To.Split(';'))
            {
                to.Trim().With(address =>
                {
                    if (address.Length > 0)
                        email.To.Add(MailboxAddress.Parse(address));
                });
            }

            email.Subject = template.Subject;
            email.Body = new TextPart(template.TextFormatHtml ? TextFormat.Html : TextFormat.Text) { Text = template.Body };
            email.Importance = (MessageImportance)template.Importance;
            email.XPriority = (XMessagePriority)template.Priority;
            email.MessageId = MimeUtils.GenerateMessageId();

            // Send email
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, sock);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(email);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Log error
            var user = _commonServices.GetUserIdAndName();
            _commonServices.StdLogException(new LogException(GetType(), ex), user.userId, user.userName,
                new Dictionary<string, object> {
                    { nameof(template.Subject), template?.Subject },
                    { nameof(template.From), template?.From },
                    { nameof(template.To), template?.To },
                    { nameof(template.TimeZone), template?.TimeZone },
                    { nameof(template.CultureInfo), template?.CultureInfo }
                });
        }

        return false;
    }

    #region PRIVATE
    /// <summary>
    /// Build HTML Header (including CSS)
    /// </summary>
    /// <returns>HTML Header</returns>
    private static string HtmlHeader => """
                <!DOCTYPE html>
                <html lang="en">
                <head>
                <title>Automated e-mail</title>
                <style>
                    table {
                      font-family: arial, sans-serif;
                      font-size: 11px;
                      border-collapse: collapse;
                      width: auto;
                    }
                    td, th {
                      border: 1px solid #dddddd;
                      text-align: left;
                      padding: 8px;
                    }
                    tr:nth-child(even) {
                      background-color: #dddddd;
                    }
                </style>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                </head>
                <body>
                """;

    /// <summary>
    /// Build HTML Footer
    /// </summary>
    /// <returns>HTML Footer</returns>
    private static string HtmlFooter => """
            </body>
            </html>
            """;

    /// <summary>
    /// Get mail template
    /// </summary>
    /// <param name="typeCode"></param>
    /// <returns></returns>
    private TR_MEL_EMail_Templates GetMailTemplate(string typeCode, string language)
        => _dbUniversContext.TR_MEL_EMail_Templates
                                .FirstOrDefault(e => e.sta_code.Equals(StatusLiteral.Available)
                                                        && e.lng_code == language
                                                        && e.mel_code == typeCode);

    /// <summary>
    /// Build Qualifications Table
    /// </summary>
    /// <param name="demandeId">TD_DEMANDEID</param>
    /// <returns>HTML qualifs table</returns>
    private string HtmlQualifsTable(int demandeId)
    {
        // Get Qualifs from table
        var qualifs = _dbUniversContext.TDQ_DEMANDE_QUALIFS.Where(e => e.TD_DEMANDEID == (demandeId < 1 ? 0 : demandeId));

        // Build qualifs table
        if (qualifs.Any())
        {
            var htmlStr = new StringBuilder($"""
                <b>{string.Format(_trad.Keys["DTF:Qualifications"], demandeId)}</b>
                <table>
                <thead style='background-color:#60a2b9;'>
                <tr>
                """);
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_NUM_ORDRE))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_CODE))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_NOM))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_VALEUR))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_NATURE))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_DATASET))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_TYPOLOGIE))}</th>");
            htmlStr.AppendLine($"<th scope='col'>{_dataAnnotations.Display<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS.TDQ_COMMENT))}</th>");
            htmlStr.AppendLine("""
                </tr>
                </thead>
                <tbody>
                """);

            foreach (var q in qualifs)
            {
                htmlStr.AppendLine("<tr>");
                htmlStr.AppendLine($"<td>{q.TDQ_NUM_ORDRE}</td> ");
                htmlStr.AppendLine($"<td>{q.TDQ_CODE ?? "-"}</td>");
                htmlStr.AppendLine($"<td>{q.TDQ_NOM ?? "-"}</td>");
                htmlStr.AppendLine($"<td><img src='{QualifsLiterals.GetQualifImgSrc(q.TDQ_VALEUR)}' alt='' loading='lazy'></td>");
                htmlStr.AppendLine($"<td>{q.TDQ_NATURE ?? "-"}</td>");
                htmlStr.AppendLine($"<td>{q.TDQ_DATASET ?? "-"}</td>");
                htmlStr.AppendLine($"<td>{q.TDQ_TYPOLOGIE ?? "-"}</td>");
                htmlStr.AppendLine($"<td>{q.TDQ_COMMENT ?? "-"}</td>");
                htmlStr.AppendLine("</tr>");
            }

            htmlStr.AppendLine("""
                </tbody>
                </table>
                """);

            return htmlStr.ToString();
        }

        return string.Empty;
    }
    #endregion
}