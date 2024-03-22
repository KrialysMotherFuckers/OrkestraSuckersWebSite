using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Krialys.Model;

public class AppSettings
{
    public string Environment { get; set; }
    public string WwwRoot { get; set; }

    public Smtp MailKit { get; set; }
    public LdapConnection LdapKit { get; set; }
    public FidConnection FidKit { get; set; }

    public WorkerNode WorkerNode { get; set; }
}

#region Internal

public class FidConnection
{
    public bool? UseFid { get; set; }
    public string Authority { get; set; }
    public string CallbackPath { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string ResponseType { get; set; }
    public string Scope { get; set; }
    public string ClaimActions { get; set; }
}

public class LdapConnection
{
    public bool? UseLdap { get; set; }
    public string SearchBase { get; set; }
    public string Host { get; set; }
    public string Port { get; set; }
}

public class Smtp
{
    public string Host { get; set; }
    public int? Port { get; set; }
    public string User { get; set; }
    public string Pass { get; set; }
    public string Sender { get; set; }
}

public class WorkerNode
{
    public string PathEnvVierge { get; set; }
    public string PathRessource { get; set; }
    public string PathQualif { get; set; }
    public string PathResult { get; set; }
    public string PathRessourceModele { get; set; }
    public string PathDoc { get; set; }
    public string PathCommande { get; set; }
    public string MetaSeparator { get; set; }
}

#endregion