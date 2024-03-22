using System.Diagnostics;

namespace Krialys.Orkestra.Common.Models.WorkerNode;

public class WorkerNodeExt
{
    // New Fields
    public string ServiceName { get; set; }
    public string ServerName { get; set; }
    public string ServerOs { get; set; }
    public string FileName { get; set; }
    public string Version { get; set; }
    public string Folder { get; set; }
    public string WorkingFilesStorage { get; set; }
    //public Process Process { get; set; }
    //public DriveInfo[] HDDInfo { get; set; }

    /// <summary>
    /// Whether or not //U can handle RefManager to interact with a .JAR file?
    /// </summary>
    public bool CanUseRefManagerFeature {  get; set; }
    public bool IsRefManagerRunning { get; set; }
}
