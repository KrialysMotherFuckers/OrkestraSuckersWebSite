// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Krialys.Common;
using Krialys.Common.Excel;
using Krialys.Common.Validations;
using Krialys.Shared.Contracts;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Krialys.Test;

public static class Program
{
    public record FichierRessources
    {
        public int TD_DEMANDEID { get; init; }
        public string TRD_NOM_FICHIER { get; init; }
        public string TER_PATH_RELATIF { get; init; }
    }

    // Problem : these values are 'hardcoded', can't be compared mathematically and can't be combined
    // policy.RequireClaim("MSORole", "Super-Admin", "Admin-Exploit", "Admin-Référentiels", "Créateur"));

    // Solution: use Enum using [Flag] decorator, see: https://docs.microsoft.com/fr-fr/dotnet/csharp/language-reference/builtin-types/enum
    [Flags]
    public enum Roles : byte
    {
        None = 0b_0000_0000,         // 0  no access at all
        Anonymous = 0b_0000_0001,    // 1  anonymous access (used for some public routes like Health check /hc)
        Application = 0b_0000_0010,  // 2  application (replace 'MSORole' to be 100% application agnostic)
        Creator = 0b_0000_0100,      // 4  creator
        AdminRef = 0b_0000_1000,     // 8  referential administrator
        AdminExploit = 0b_0001_0000, // 16 exploitation administrator
        SuperAdmin = 0b_0010_0000,   // 32 super administrator
        GodMode = Anonymous | Application | Creator | AdminRef | AdminExploit | SuperAdmin // 63
    }

    /// <summary>
    /// List of unique flags made from a serie
    /// </summary>
    /// <param name="flags">Flags to iterate</param>
    /// <returns></returns>
    public static IEnumerable<Enum> GetUniqueFlags(this Enum flags)
    {
        ulong flag = 1;
        foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>())
        {
            ulong bits = Convert.ToUInt64(value);
            while (flag < bits)
            {
                flag <<= 1;
            }

            if (flag == bits && flags.HasFlag(value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Test role amongst all roles assigned
    /// </summary>
    /// <param name="fromRoles">Assigned roles</param>
    /// <param name="whereIsInRole">Role to test</param>
    /// <returns></returns>
    public static bool IsInRole(this Roles fromRoles, Roles whereIsInRole)
        => fromRoles.GetUniqueFlags().Any(x => x.CompareTo(whereIsInRole) == 0);

    private static void TestRoles()
    {
        // Assign roles
        var assignedRoles = Roles.Anonymous | Roles.Application | Roles.Creator | Roles.AdminRef | Roles.AdminExploit;

        // Get each assigned role (useful for checking a profile)
        Roles cumulatedRoles = default;
        foreach (var flag in assignedRoles.GetUniqueFlags())
        {
            Console.WriteLine($"One of my role is: {(flag)}");     // => Anonymous, Application, Creator, AdminRef, AdminExploit
            cumulatedRoles |= (Roles)flag;
        }
        Console.WriteLine($"My role are: {(cumulatedRoles)}");     // => Anonymous, Application, Creator, AdminRef, AdminExploit

        // Check if we have a AdminRef role
        Console.WriteLine($"Has AdminRef role: {assignedRoles.IsInRole(Roles.AdminRef)}");   // => true

        // Check if we have a Anonymous role
        Console.WriteLine($"Has Anonymous role: {assignedRoles.IsInRole(Roles.Anonymous)}"); // => true

        // Check if we have a GodMode role
        Console.WriteLine($"Has GodMode role: {assignedRoles.IsInRole(Roles.GodMode)}");     // => false

        // Cast Roles to integer, then decode
        var roles1 = (Roles)6;
        Console.WriteLine($"My roles are: {(roles1)}"); // => Application, Creator

        // Once again
        var roles2 = (Roles)17;
        Console.WriteLine($"My roles are: {(roles2)}"); // => Anonymous, AdminExploit

        // Last test
        var roles3 = (Roles)63;
        Console.WriteLine($"My roles are: {(roles3)}"); // => GodMode
    }

    private static void CircularBufferTest()
    {
        var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2 });
        Console.WriteLine("Initial buffer {0,1,2}:");
        PrintBuffer(buffer);

        //buffer.PushFront(15);
        //PrintBuffer(buffer);
        //buffer.PushBack(14);
        //PrintBuffer(buffer);

        //buffer.Where(x => x > 13).ToList().ForEach(x => Console.WriteLine(x));

        for (int i = 3; i < 7; i++)
        {
            buffer.PushBack(i);
        }
        Console.WriteLine("\nAfter adding a 7 elements to a 5 elements capacity buffer:");
        PrintBuffer(buffer);


        buffer.PopFront();
        Console.WriteLine("\nbuffer.PopFront():");
        PrintBuffer(buffer);


        buffer.PopBack();
        Console.WriteLine("\nbuffer.PopBack():");
        PrintBuffer(buffer);

        for (int i = 2; i >= 0; i--)
        {
            buffer.PushFront(i);
        }
        Console.WriteLine("\nbuffer.PushFront() {2,1,0} respectively:");
        PrintBuffer(buffer);

        buffer.Clear();
        Console.WriteLine("\nbuffer.Clear():");
        PrintBuffer(buffer);
    }

    private static void PrintBuffer(CircularBuffer<int> buffer)
    {
        Console.WriteLine($"{{{string.Join(",", buffer.ToArray())}}}");
    }

    public class TestControlDateTime
    {
        [ControlDateTime(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, ErrorMessage = "The {0} isn't valid")]
        public DateTime EntryDate { get; set; }
    }
    private static void ControlDateTimeTest()
    {
        var t = new TestControlDateTime
        {
            EntryDate = DateTime.Now.AddDays(3)
        };
        var err = t.ValidateObject();
        var errors = err.GetAllErrors();

        t.EntryDate = DateTime.Now.AddDays(1);
        err = t.ValidateObject();
        errors = err.GetAllErrors();

        //var error = errors.First().ErrorMessage; //, $"The property {nameof(textclass.ArrayInt)} doesn't have more than 2 elements");
    }

    private static async Task Main()
    {
        await LDAPTest.TestLdapAsync();

        //            RoslynTest.Compiler();

        //            ControlDateTimeTest();

        //            CircularBufferTest();

        //            var time = DateExtensions.GetUtcNow();
        //            time = time.Truncate(TimeSpan.FromMinutes(1));

        //            string posix = PosixTimeZone.FromTimeZoneInfo(TimeZoneInfo.Local);

        //            // Either of these will work on any platform:
        //            TimeZoneInfo tzi1 = TZConvert.GetTimeZoneInfo("Romance Standard Time");
        //            TimeZoneInfo tzi2 = TZConvert.GetTimeZoneInfo("Europe/Paris");

        //            DateTime localDateTime01 = DateExtensions.ConvertToTimeZoneFromUtc(DateTime.UtcNow, "Romance Standard Time");
        //            DateTime localDateTime02 = DateExtensions.ConvertToTimeZoneFromUtc(DateTime.UtcNow, "Europe/Paris");

        //            TestRoles();

        //            /// <summary>Validates format of given value.</summary>
        //            /// <param name="value">String value.</param>
        //            /// <returns>True if value is correct for given format, False - if not.</returns>
        //#pragma warning disable CS8321 // The local function 'IsValidMail' is declared but never used
        //            static bool IsValidMail(string value)
        //#pragma warning restore CS8321 // The local function 'IsValidMail' is declared but never used
        //            {
        //                return Regex.IsMatch(value,
        //                    @"^\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z$",
        //                    RegexOptions.IgnoreCase);
        //            }

        //            var files1 =
        //                    from dir in Directory.EnumerateDirectories(@"C:\mp3DirectCut\")
        //                    from file in Directory.EnumerateFiles(dir)
        //                    select file;

        //            var files2 = Directory.EnumerateDirectories(@"C:\mp3DirectCut\")
        //                .SelectMany(dir => Directory.EnumerateFiles(dir));

        //// Test #1: TPS_PREREQUIS_SCENARIOS -> (TEP_ETAT_PREREQUISID, TS_SCENARIOID)
        //// Test #2: TRUCL_USERS_CLAIMS -> (TRCL_CLAIMID, TRU_USERID)
        //// Test #3: TRU_USERS -> (y'en a pas)
        //var listFk1 = Krialys.Entities.COMMON.Extensions.GetFkList<TPS_PREREQUIS_SCENARIOS>();
        //var listFk2 = Krialys.Entities.COMMON.Extensions.GetFkList<TRUCL_USERS_CLAIMS>();
        //var listFk3 = Krialys.Entities.COMMON.Extensions.GetFkList<TRU_USERS>();

        //var t = (yy[0].CustomAttributes).ConstructorArguments;

        //var fk = (from property in typeof(TEntity).GetType().GetProperties()
        //          where Attribute.IsDefined(property, typeof(KeyAttribute))
        //          orderby ((ColumnAttribute)property.GetCustomAttributes(false).Single(
        //              attr => attr is ColumnAttribute)).Order ascending
        //          select property.GetValue(typeof(TEntity))).ToArray();

        #region Authentification

        //var dt = GetDateTimeOffset("2021-03-18 23:34:48.9521106+01:00");
        //dt = GetDateTimeOffset("2021-03-18 23:34:48.9521106 +01:00");
        //dt = GetDateTimeOffset("2021-12-31 09:45:00 -02:00");
        //dt = GetDateTimeOffset("2021-03-18 22:30:00+01:00");

        //await SimulateParalellUClient();

        #endregion Authentification

        //TestDynamicContracts();

        TestContractValidator();

        //TestAsyncMso().GetAwaiter().GetResult();
        //TestAsyncProducts().GetAwaiter().GetResult();

        //TestSafeDictionary.Test();

        await RunAsync();
    }

    //private static DateTimeOffset GetDateTimeOffset(string dateTimeOffsetString)
    //{
    //    dateTimeOffsetString = dateTimeOffsetString.Trim();

    //    var dateTime = DateTimeOffset.ParseExact(
    //        dateTimeOffsetString,
    //        new[]
    //        {
    //            "yyyy-MM-dd HH:mm:ss.fffffff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.fffffffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.ffffff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.ffffffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.fffff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.fffffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.ffff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.ffffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.fff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.fffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.ff zzzz",
    //            "yyyy-MM-dd HH:mm:ss.ffzzzz",
    //            "yyyy-MM-dd HH:mm:ss.f zzzz",
    //            "yyyy-MM-dd HH:mm:ss.fzzzz",
    //            "yyyy-MM-dd HH:mm:ss zzzz",
    //            "yyyy-MM-dd HH:mm:sszzzz",
    //            "yyyy-MM-dd HH:mm zzzz",
    //            "yyyy-MM-dd HH:mmzzzz",
    //        },
    //        CultureInfo.InvariantCulture, DateTimeStyles.None);

    //    return dateTime;
    //}

    private static void TestContractValidator()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        for (int j = 0; j < 1000; j++)
        {
            ContractValidator.Test();
        }

        stopwatch.Stop();
        Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}");

        /* => Generated Json schema
        {
          "$schema": "http://json-schema.org/draft-04/schema#",
          "id": "CTX_TEST_v001",
          "type": "object",
          "additionalProperties": false,
          "required": [
            "TR_NAME",
            "TR_EMAIL",
            "TR_DATE"
          ],
          "properties": {
            "TR_TESTID": {
              "type": "number",
              "format": "int64"
            },
            "TR_AGE": {
              "type": "integer",
              "format": "int32"
            },
            "TR_NAME": {
              "type": "string",
              "maxLength": 16,
              "minLength": 2
            },
            "TR_EMAIL": {
              "type": "string",
              "format": "email",
              "maxLength": 16,
              "minLength": 5
            },
            "TR_DATE": {
              "type": "string",
              "format": "date-time"
            }
          }
        }
         */
    }

    private static void TestDynamicContracts()
    {
        Stopwatch stopwatch = new();

        Console.Clear();


        // 1- Crée une instance de DynamicContracts
        using var contract = DynamicContracts.CreateInstance();

        // 2 - Récupère le contrat d'interface adéquat (issu d'une table référentielle à construire, à voir avec Seb)
        string contrat = "Id de test;int;Name;string;Age;int;Date;datetime;";

        // 3 - Récupère le champ contenant le flux Json à challenger (issu du dernier champ de la table TT_LOG)
        //     ex. valide => {"Id":2,"Age":35,"Name":null,"Date":"2020-07-03T09:30:00+02:00"}
        //     ex. valide => {"Id":2,"Age":35,"Date":"2020-07-03T09:30:00+02:00"}

        stopwatch.Start();

        //var uu = Extensions.ToExpandoObject(new { Id = 123, Text = "Abc123", Test = true });
        // Test sur 1 million : =~ 25 000 contrats / sec décodés
        for (int i = 0; i < 1000000; i++)
        {
            string input = Body(i);

            // 4 - Crée un ExpandoObject à partir du croisement des étapes 2 et 3
            var expando = contract
                .Select(contrat)
                .From(input);

            //var json = expando.GetFieldValue("Age");

            expando.ToJson();

            // 5 - Récupère l'erreur lorsque expando est null
        }

        stopwatch.Stop();
        _ = $"Time elapsed: {stopwatch.Elapsed}";
    }

    // In real life, the body comes from database
    private static string Body(int i)
    {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);

        writer.WriteStartObject();
        writer.WriteNumber("Id de test", 1 + i);
        writer.WriteNumber("Age", 34 + i);
        writer.WriteString("Name", "Gérôme");
        //writer.WriteNull("Name");
        //writer.WriteNull("Sex");
        //writer.WriteNumber("Sex", 42.42 + i);
        writer.WriteString("Date", DateTimeOffset.Parse("2020-07-03 09:30:00+02:00", CultureInfo.InvariantCulture)); // z
        writer.WriteEndObject();
        writer.Flush();

        //byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(ms.ToArray());
        //var result = JsonSerializer.Deserialize(ms.ToArray(), typeof(ExpandoObject)) as ExpandoObject;

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static async Task RunAsync()
    {
        const bool bulkMode = true;
        //string error;

        new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8000/")
        };

        Console.WriteLine("> **************************** ALL STARTS HERE *************************************");
        var sw = Stopwatch.StartNew();

        sw.Start();

        // Create instance
        var instance = ExcelReader.CreateInstance();

        // Version courte : mapping sur la classe 'MappingQualif'
        //await instance.Load<CPU.MappingQualifDemande>(@"C:\KRepertoireTravail_DEV\Qualif.csv", "");

        // Version longue : mapping dynamique basé sur les noms de colonnes, données non typées
        var queryableDicoQualif = await instance.Load(@"C:\KRepertoireTravail_DEV\Qualif.csv", "",
            new[] {
                "CODE_QUALIF",
                "NOM_QUALIF",
                "CODE_ETAT_QUALIF",
                "VALEUR_QUALIF",
                "NATURE_QUALIF",
                "DATASET_QUALIF",
                "OBJET_QUALIF",
                "COMMENT_QUALIF",
                "DATE_PROD_QUALIF",
                "ID_DEMANDE"
            });

        // Puis transposition en List<MappingQualif>
        //var listMappingQualif = queryableDicoQualif.Select(el => instance.Map<CPU.MappingQualifDemande>(el)).ToList();

        //var products = await instance.Load<PRODUCTS>(@"C:\FRST\Products - Copie.xlsx", "Produits frais");
        //error = instance.GetLastError();

        /*
             var dico = await instance.Load(@"C:\FRST\SampleA.xls", "Spreadsheet-file", new[] { "Id", "Key", "Name", "Category", "Quantity", "Comment" });
             error = instance.GetLastError();

             // Converti KV vers PRODUCTS
             List<PRODUCTS> products = new List<PRODUCTS>();
             foreach (var el in dico)
             {
                 products.Add(instance.Map<PRODUCTS>(el));
             }
         */

        //var timesec = sw.ElapsedMilliseconds < 1000 ? 1 : sw.ElapsedMilliseconds / 1000;
        //Console.WriteLine($"> Read {1 * products.Count()} products in {timesec} seconds. Speed rate: {1 * products.Count() / timesec} lines read per second");
        //Console.WriteLine("> **********************************************************************************");
        sw.Reset();
        Console.WriteLine($"> Bulk : {bulkMode}");

        sw.Start();
        //var nbUpdated = bulkMode ? await Proxy.UpdateBulkAsync(products) : await Proxy.UpdateAsync(products);
        //var nbAdded = await proxy.CreateAsync(products, bulkInsert: 1);
        //var nbDeleted = bulkMode ? await Proxy.DeleteBulkAsync(new string[] { "11", "12", "13", "14" }) : await Proxy.DeleteAsync(new string[] { "11", "12", "13", "14" });

        //var milliseconds = sw.ElapsedMilliseconds < 1000 ? 1 : sw.ElapsedMilliseconds / 1000;
        //Console.WriteLine($"> Created {(string.IsNullOrEmpty(nbProducts) ? 0 : nbProducts.Split(",").Length)} products in {milliseconds} seconds. Speed rate: {products.Count() / milliseconds} lines per second");
        //Console.WriteLine("> **********************************************************************************");
        sw.Reset();

        //var rangeToUpdate = products.Take(2);  //((List<PRODUCTS>)products).GetRange(1, 2); // ((List<PRODUCTS>)products).Count - 1);
        //foreach (var v in rangeToUpdate)
        //{
        //    v.Quantity = -420;
        //}
        //var nbUpdated = await proxy.UpdateAsync(rangeToUpdate, bulkUpdate: 1);

        //sw.Start();
        //var RangeToUpdate = products.GetRange(1, products.Count - 1);
        //foreach (var v in RangeToUpdate)
        //{
        //    v.Quantity = -42;
        //}
        //var nbUpdated = bulkMode ? await Proxy.UpdateBulkAsync(RangeToUpdate) : await Proxy.UpdateAsync(RangeToUpdate);
        //milliseconds = sw.ElapsedMilliseconds < 1000 ? 1 : sw.ElapsedMilliseconds / 1000;
        //Console.WriteLine($"> Updated {nbUpdated} products updated in {milliseconds} seconds. Speed rate: {products.Count / milliseconds} lines per second");
        //Console.WriteLine("> **********************************************************************************");
        //sw.Reset();

        //sw.Start();
        //// Delete 4 products
        //var nbDeleted = bulkMode ? await Proxy.DeleteBulkAsync(new string[] { "11", "12", "13", "14" }) : await Proxy.DeleteAsync(new string[] { "11", "12", "13", "14" });
        //milliseconds = sw.ElapsedMilliseconds < 1000 ? sw.ElapsedMilliseconds : sw.ElapsedMilliseconds / 1000;
        //Console.WriteLine($"> Deleted {nbDeleted} products updated in {milliseconds} {(sw.ElapsedMilliseconds < 1000 ? "milliseconds" : "seconds")}. Speed rate: {products.Count / milliseconds} lines per second");
        //Console.WriteLine("> **********************************************************************************");

        sw.Stop();
        Console.Write("Type any key to continue...");
        Console.ReadLine();
    }
}