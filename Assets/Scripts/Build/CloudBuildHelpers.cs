using System.IO;
using UnityEditor;
using UnityEngine;

///<summary>
/// Utilidades para facilitar builds en la nube (Cloud Build) en Unity.
/// Incluye métodos para integración y configuración de builds automáticos.
/// </summary>
public static class CloudBuildHelpers
{
    const string k_AdditionalClientBuildFilesFolder = "AdditionalBuildFiles/Client/";
    const string k_AdditionalServerBuildFilesFolder = "AdditionalBuildFiles/Server/";

    /// <summary>
    /// Configura parámetros específicos para Cloud Build.
    /// </summary>
    public static void ConfigureCloudBuild()
    {
        // Lógica para configurar parámetros de Cloud Build
    }

    /// <summary>
    /// Ejecuta un build en la nube con la configuración actual.
    /// </summary>
    public static void RunCloudBuild()
    {
        // Lógica para lanzar el build en la nube
    }

    /// <summary>
    /// Method called from CloudBuild when the build finishes.
    /// Needs to be referenced in the settings in CloudBuild's dashboard
    /// </summary>
    /// <param name="exportPath">The path where the build is</param>
    /// <param name="isServerBuild">Is this a server build?</param>
    public static void PostExport(string exportPath, bool isServerBuild)
    {
        FileAttributes attr = File.GetAttributes(exportPath);
        string directory;
        if (attr.HasFlag(FileAttributes.Directory))
        {
            directory = exportPath;
        }
        else
        {
            directory = Path.GetDirectoryName(exportPath);
        }
        CopyDirectory(isServerBuild ? k_AdditionalServerBuildFilesFolder : k_AdditionalClientBuildFilesFolder, directory, true);
    }

    static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        var dir = new DirectoryInfo(sourceDirName);
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
        }

        // If the destination directory doesn't exist, create it.
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        foreach (FileInfo file in dir.GetFiles())
        {
            file.CopyTo(Path.Combine(destDirName, file.Name), true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                CopyDirectory(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
            }
        }
    }
}