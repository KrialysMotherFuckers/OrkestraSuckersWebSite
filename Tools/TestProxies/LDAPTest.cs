using LdapForNet;
using LdapForNet.Native;

namespace Krialys.Test;

// https://github.com/flamencist/ldap4net #searchbycn
public static class LDAPTest
{
    public static async Task TestLdapAsync()
    {
        try
        {
            var host = "ldap.forumsys.com";
            var port = 389;
            var @base = "dc=example,dc=com"; // à priori dc=COMMUN
            var filter = "(objectclass=*)";  // à priori "(SAMAccountName=" + PSBYxxx + ")"

            using (var cn = new LdapConnection())
            {
                cn.Connect(host, port);                           // à priori cn.Connect(new Uri("ldap://COMMUN:389"));
                var who = "cn=read-only-admin,dc=example,dc=com"; // à priori cn=PSBYxxx
                var password = "password";
                cn.Bind(Native.LdapAuthMechanism.SIMPLE, who, password);

                var entries = await cn.SearchAsync(@base, filter);

                foreach (var ldapEntry in entries)
                {
                    PrintEntry(ldapEntry);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }

    private static void PrintEntry(LdapEntry entry)
    {
        Console.WriteLine($"dn: {entry.Dn}");
#pragma warning disable CS0612 // Type or member is obsolete
        foreach (var pair in entry.Attributes.SelectMany(_ => _.Value.Select(x => new KeyValuePair<string, string>(_.Key, x))))
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
#pragma warning restore CS0612 // Type or member is obsolete
        Console.WriteLine();
    }
}

////le domaine d'authentification des users dans le domaine peut etre différent du domaine dans lequel le serveur est hébergé
//Imports System.DirectoryServices
// Private Function IsAuthenticatedLDAP(domain As String, username As String, pwd As String) As Boolean

//        Try
//            Dim strLDAP = "LDAP://" & domain
//            Dim domainAndUsername As String = domain + "\" + username
//            Dim entry As DirectoryEntry = New DirectoryEntry(strLDAP,
//                                             domainAndUsername,
//                                               pwd, AuthenticationTypes.Secure)

//            Dim obj As Object = entry.NativeObject
//            Dim search As DirectorySearcher = New DirectorySearcher(entry)
//            search.Filter = "(SAMAccountName=" + username + ")"
//            search.PropertiesToLoad.Add("cn")
//            Dim result As SearchResult = search.FindOne()
//            If IsNothing(result) Then
//                Return False
//            Else
//                Return True
//            End If

//        Catch ex As Exception
//            Return False

//        End Try
//    End Function

////  methode qui permet de rechercher des infos sur un utilisateur dans l'annuaire
//  Public Function LdapGetAllUsers(ByVal protectKey As String, ByVal UserLogin As String, ByRef UserInfo As String) As Boolean Implements IServiceDB.LdapGetAllUsers
//        If chkSecurity(protectKey) = False Then Throw New System.Exception("Security control - Unauthorized.")

//        'retourne false si login pas trouvé ou compte non ouvert
//        Dim results As SearchResultCollection = Nothing
//        Dim usertrouvé As Boolean = False

//        Try
//            Dim oRoot As New DirectoryServices.DirectoryEntry("LDAP://RootDSE")
//            Dim sDomain As String = oRoot.Properties("DefaultNamingContext").Value
//            Dim strLDAP = "LDAP://" & sDomain
//            Dim nom, prenom, email, compteDesactive, Departement As String
//            nom = ""
//            prenom = ""
//            email = ""
//            compteDesactive = ""
//            Departement = ""

//            ' Bind to the users container.
//            Dim path As String
//            path = strLDAP '"LDAP://CN=Users,DC=strohmadom,DC=nttest,DC=microsoft,DC=com"
//            Dim entry As New DirectoryEntry(path)

//            ' Create a DirectorySearcher object.
//            Dim mySearcher As New DirectorySearcher(entry)

//            ' Set a filter for users with the name test.
//            mySearcher.Filter = "(&(objectClass=user)(sAMAccountName=" & UserLogin & "))"
//            mySearcher.SearchScope = SearchScope.Subtree

//            ' Use the FindAll method to return objects to a SearchResultCollection.
//            results = mySearcher.FindAll()

//            ' Iterate through each SearchResult in the SearchResultCollection.
//            Dim searchResult As SearchResult
//            For Each searchResult In results
//                usertrouvé = True
//                ' Display the path of the object found.
//                'Console.WriteLine("Search properties for {0}",searchResult.Path)

//                ' Iterate through each property name in each SearchResult.
//                Dim propertyKey As String
//                For Each propertyKey In searchResult.Properties.PropertyNames
//                    ' Retrieve the value assigned to that property name 
//                    ' in the ResultPropertyValueCollection.
//                    Dim valueCollection As ResultPropertyValueCollection = searchResult.Properties(propertyKey)

//                    ' Iterate through values for each property name in each SearchResult.
//                    Dim propertyValue As Object
//                    For Each propertyValue In valueCollection
//                        If propertyKey = "useraccountcontrol" Then 'AccountDisabled" Then

//                            If propertyValue = 512 Or propertyValue = 544 Or propertyValue = 66048 Then
//                                '512 - Enable Account
//                                '544 - Account Enabled - Require user to change password at first logon
//                                '66048 - Enabled, password never expires
//                                compteDesactive = "non"
//                            Else : compteDesactive = "oui(" + propertyValue.ToString() + ")"
//                            End If

//                        ElseIf propertyKey = "mail" Then 'ok
//                            email = propertyValue.ToString()
//                        ElseIf propertyKey = "givenname" Then 'prenom OK
//                            prenom = propertyValue.ToString()
//                        ElseIf propertyKey = "sn" Then 'nom famille ok
//                            nom = propertyValue.ToString()
//                        ElseIf propertyKey = "title" Then 'Departement 
//                            Departement = propertyValue.ToString()
//                        End If

//                    Next propertyValue
//                Next propertyKey
//            Next searchResult

//            UserInfo = compteDesactive & "¤" & nom & "¤" & prenom & "¤" & email & "¤" & Departement
//        Catch ex As Exception

//            UserInfo = ex.Message
//        Finally
//            ' To prevent memory leaks, always call 
//            ' SearchResultCollection.Dispose() manually.
//            If Not results Is Nothing Then
//                results.Dispose()
//                results = Nothing
//            End If
//            LdapGetAllUsers = usertrouvé
//        End Try

//    End Function