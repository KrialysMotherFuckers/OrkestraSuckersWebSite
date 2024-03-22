using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models.Email;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
/// Implements dedicated controller for sending EMAIL via SMTP
/// Nuget: https://github.com/jstedfast/MailKit
/// Example: https://jasonwatmore.com/post/2021/09/02/net-5-send-an-email-via-smtp-with-mailkit
/// </summary>
[ApiExplorerSettings]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    //private readonly AppSettings _appSettings;
    private readonly IEmailServices _emailServices;

    public EmailController(IEmailServices emailServices)
        => _emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));

    /// <summary>
    /// Send mail<br />
    /// [!] Errors are logged to logUnivers.
    /// </summary>
    /// <param name="template">Email parameters and content</param>
    /// <returns></returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("SendGenericMail")]
    public Task<bool> SendGenericMailAsync(EmailTemplate template)
        => _emailServices.SendGenericMail(template);

    /// <summary>
    /// Send automated mail<br />
    /// [!] Errors are logged to logUnivers.
    /// </summary>
    /// <param name="demandeId"></param>
    /// <param name="typeCode"></param>
    /// <returns></returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("SendAutomatedMailForRequest")]
    public Task<bool> SendAutomatedMailAsync(int demandeId, string typeCode)
        => _emailServices.SendAutomatedMailForRequest(demandeId, typeCode);

    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [HttpPost("SendAutomatedMailForOrder")]
    public Task<bool> SendAutomatedMailForOrderAsync(int orderId, string statusTypeCode)
        => _emailServices.SendAutomatedMailForOrder(orderId, statusTypeCode);

    /// <summary>
    /// Gets the email template list.
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("GetEmailTemplateList")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TR_MEL_EMail_Templates>))]
    public Task<IEnumerable<TR_MEL_EMail_Templates>> GetEmailTemplateList() => _emailServices.GetEmailTemplateList();
}