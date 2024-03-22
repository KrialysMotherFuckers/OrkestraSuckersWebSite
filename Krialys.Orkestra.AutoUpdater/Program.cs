using Krialys.Orkestra.AutoUpdater;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace Krialys.Orkestra.ParallelU.AutoUpdater;

internal class Program
{
    static void Main(string[] args)
    {
        //args = new string[] {
        //    "update",
        //    @"C:\MyWork\Krialys\Krialys.Orkestra.ParallelU\bin\Debug\net6.0\Krialys.Orkestra.ParallelU.exe.update",
        //    @"C:\MyWork\Krialys\Krialys.Orkestra.ParallelU\bin\Debug\net6.0\Krialys.Orkestra.ParallelU.exe",
        //    ""
        //};

        if (args.Length == 0)
        {
            Console.WriteLine("Invalid args");
            return;
        }

        switch (args[0].ToLower())
        {
            case "update" when args.Length >= 4:
                Core.ExecuteUpdateProcess(args[1], args[2], args[3], (args.Length == 5 ? args[4] : ""));
                break;
            default:
                Console.WriteLine("Invalid command");
                break;
        }
    }
}
