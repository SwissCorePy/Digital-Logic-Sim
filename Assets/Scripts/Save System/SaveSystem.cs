using System;
using System.Diagnostics;
using System.IO;
using Core;
using Save_System.Serializable;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Save_System
{
public static class SaveSystem
{
    private const string FileExtension = ".txt";
    private static string _activeProjectName = "Untitled";

    private static string CurrentSaveProfileDirectoryPath =>
    Path.Combine(SaveDataDirectoryPath, _activeProjectName);

    private static string CurrentSaveProfileWireLayoutDirectoryPath =>
    Path.Combine(CurrentSaveProfileDirectoryPath, "WireLayout");

    private static string SaveDataDirectoryPath
    {
        get
        {
            const string saveFolderName = "SaveData";
            return Path.Combine(Application.persistentDataPath, saveFolderName);
        }
    }

    public static void SetActiveProject(string projectName)
    {
        _activeProjectName = projectName;
    }

    public static void Init()
    {
        // Create save directory (if doesn't exist already)
        Directory.CreateDirectory(CurrentSaveProfileDirectoryPath);
        Directory.CreateDirectory(CurrentSaveProfileWireLayoutDirectoryPath);
    }

    private static string[] GetChipSavePaths()
    {
        return Directory.GetFiles(CurrentSaveProfileDirectoryPath, "*" + FileExtension);
    }

    public static void LoadAll(Manager manager)
    {
        // Load any saved chips
        var sw = Stopwatch.StartNew();
        ChipLoader.LoadAllChips(GetChipSavePaths(), manager);
        Debug.Log("Load time: " + sw.ElapsedMilliseconds);
    }

    public static SavedChip[] GetAllSavedChips()
    {
        // Load any saved chips
        return ChipLoader.GetAllSavedChips(GetChipSavePaths());
    }

    public static string GetPathToSaveFile(string saveFileName)
    {
        return Path.Combine(CurrentSaveProfileDirectoryPath, saveFileName + FileExtension);
    }

    public static string GetPathToWireSaveFile(string saveFileName)
    {
        return Path.Combine(CurrentSaveProfileWireLayoutDirectoryPath, saveFileName + FileExtension);
    }

    public static string[] GetSaveNames()
    {
        var savedProjectPaths = Array.Empty<string>();
        if (Directory.Exists(SaveDataDirectoryPath))
            savedProjectPaths = Directory.GetDirectories(SaveDataDirectoryPath);
        for (var i = 0; i < savedProjectPaths.Length; i++)
        {
            var pathSections = savedProjectPaths[i].Split(Path.DirectorySeparatorChar);
            savedProjectPaths[i] = pathSections[pathSections.Length - 1];
        }

        return savedProjectPaths;
    }
}
}