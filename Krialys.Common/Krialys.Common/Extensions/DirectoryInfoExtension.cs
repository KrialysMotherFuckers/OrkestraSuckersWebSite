namespace Krialys.Common.Extensions;

public static class DirectoryInfoExtension
{
    public static void DeepCopy(this DirectoryInfo directory, string destinationDir)
    {
        foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
        {
            string dirToCreate = dir.Replace(directory.FullName, destinationDir);
            Directory.CreateDirectory(dirToCreate);
        }

        foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(directory.FullName, destinationDir), true);
        }
    }
}
