namespace Krialys.Test;

public static class Program
{
    private static async Task Main()
    {
        await LDAPTest.TestLdapAsync();
    }
}