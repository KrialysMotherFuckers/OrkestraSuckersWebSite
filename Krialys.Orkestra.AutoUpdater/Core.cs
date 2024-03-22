using System.Diagnostics;

namespace Krialys.Orkestra.AutoUpdater;

public static class Core
{
    public static void ExecuteUpdateProcess(string pathFileUpdate, string pathFileToUpdate, string processId, string serviceName)
    {
#if Release
        Thread.Sleep(5000);
#endif

        if (!string.IsNullOrEmpty(serviceName)) StopWindowsService(serviceName);
        else KillProcess(Convert.ToInt32(processId));

        UdpateFile(pathFileUpdate, pathFileToUpdate);

        if (!string.IsNullOrEmpty(serviceName)) StartWindowsService(serviceName);
        else StartNewExecutable(pathFileToUpdate);
    }

    public static void StopWindowsService(string serviceName)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo("net.exe", "stop " + serviceName);
            processStartInfo.UseShellExecute = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.WorkingDirectory = Environment.SystemDirectory;
            var start = Process.Start(processStartInfo);
            start?.WaitForExit();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static void StartWindowsService(string serviceName)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo("net.exe", "start " + serviceName);
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.WorkingDirectory = Environment.SystemDirectory;
            var start = Process.Start(processStartInfo);
            start?.WaitForExit();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static void KillProcess(int pid)
    {
        if (pid == 0) return;

        try
        {
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            //ManagementObjectCollection moc = searcher.Get();
            //foreach (ManagementObject mo in moc)
            //{
            //    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            //}

            try
            {
                var proc = System.Diagnostics.Process.GetProcessById(pid);
                proc.CloseMainWindow();
                proc.WaitForExit(1000);
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }

        }
        catch (Exception)
        {

            throw;
        }
    }

    public static void UdpateFile(string pathFileUpdate, string pathFileToUpdate)
    {
        try
        {
            Thread.Sleep(5000);

            Console.WriteLine("Starting Update.");

            if (File.Exists(pathFileUpdate) && File.Exists(pathFileToUpdate))
            {
                Console.WriteLine($"Create Executable BackUp : {pathFileToUpdate}_backup");
                if (File.Exists($"{pathFileToUpdate}_backup")) File.Delete($"{pathFileToUpdate}_backup");
                File.Copy(pathFileToUpdate, $"{pathFileToUpdate}_backup", true);

                Console.WriteLine($"Delete Old Executable: {pathFileToUpdate}");
                File.Delete(pathFileToUpdate);
                Thread.Sleep(5000);

                Console.WriteLine($"Update Current Executable with new Update File");
                File.Move(pathFileUpdate, pathFileToUpdate, true);

                Console.WriteLine($"Delete Update File: {pathFileUpdate}");
                File.Delete(pathFileUpdate);

                Console.WriteLine($"Delete Executable BackUp : {pathFileToUpdate}_backup");
                File.Delete($"{pathFileToUpdate}_backup");

                Console.WriteLine("Update done...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public static void StartNewExecutable(string pathFileUpdate)
    {
        try
        {
            System.Diagnostics.Process.Start
            (
                new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = pathFileUpdate,

                    UseShellExecute = true,

                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,

                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                    WorkingDirectory = Path.GetFullPath(pathFileUpdate),
                }
            )!.Start();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
