using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using Graphics;
using Save_System.Serializable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Save_System {
public static class ChipSaver {
  private const bool UsePrettyPrint = true;

  public static void Save(ChipEditor chipEditor) {
    var chipSaveData = new ChipSaveData(chipEditor);

    // Generate new chip save string
    var compositeChip = new SavedChip(chipSaveData);
    var saveString = JsonUtility.ToJson(compositeChip, UsePrettyPrint);

    // Generate save string for wire layout
    var wiringSystem = new SavedWireLayout(chipSaveData);
    var wiringSaveString = JsonUtility.ToJson(wiringSystem, UsePrettyPrint);

    // Write to file
    var savePath = SaveSystem.GetPathToSaveFile(chipEditor.chipName);
    using (var writer = new StreamWriter(savePath)) {
      writer.Write(saveString);
    }

    var wireLayoutSavePath =
        SaveSystem.GetPathToWireSaveFile(chipEditor.chipName);
    using (var writer = new StreamWriter(wireLayoutSavePath)) {
      writer.Write(wiringSaveString);
    }
  }

  public static void Export(Chip.Chip exportedChip, string destinationPath) {
    var chipsToExport = FindChildrenChips(exportedChip.chipName);

    using var writer = new StreamWriter(destinationPath);

    writer.WriteLine(chipsToExport.Count);

    foreach (var chip in chipsToExport.OrderBy(x => x.Key)) {
      var chipSaveFile = SaveSystem.GetPathToSaveFile(chip.Value);
      var chipWireSaveFile = SaveSystem.GetPathToWireSaveFile(chip.Value);

      using var reader = new StreamReader(chipSaveFile);

      var saveString = reader.ReadToEnd();

      using var wireReader = new StreamReader(chipWireSaveFile);

      var wiringSaveString = wireReader.ReadToEnd();

      writer.WriteLine(chip.Value);
      writer.WriteLine(saveString.Split('\n').Length);
      writer.WriteLine(wiringSaveString.Split('\n').Length);
      writer.WriteLine(saveString);
      writer.WriteLine(wiringSaveString);
    }
  }

  private static Dictionary<int, string> FindChildrenChips(string chipName) {
    var childrenChips = new Dictionary<int, string>();

    var manager = Object.FindObjectOfType<Manager>();
    var allChips = SaveSystem.GetAllSavedChips();
    var currentChip = Array.Find(allChips, c => c.name == chipName);
    if (currentChip == null)
      return childrenChips;

    childrenChips.Add(currentChip.creationIndex, chipName);

    foreach (var scc in currentChip.savedComponentChips) {
      if (Array.FindIndex(manager.builtinChips,
                          c => c.chipName == scc.chipName) != -1)
        continue;

      foreach (var chip in FindChildrenChips(scc.chipName)
                   .Where(chip => !childrenChips.ContainsKey(chip.Key)))
        childrenChips.Add(chip.Key, chip.Value);
    }

    return childrenChips;
  }

  public static void Update(ChipEditor chipEditor, Chip.Chip chip) {
    var chipSaveData = new ChipSaveData(chipEditor);

    // Generate new chip save string
    var compositeChip = new SavedChip(chipSaveData);
    var saveString = JsonUtility.ToJson(compositeChip, UsePrettyPrint);

    // Generate save string for wire layout
    var wiringSystem = new SavedWireLayout(chipSaveData);
    var wiringSaveString = JsonUtility.ToJson(wiringSystem, UsePrettyPrint);

    // Write to file
    var savePath = SaveSystem.GetPathToSaveFile(chipEditor.chipName);
    using (var writer = new StreamWriter(savePath)) {
      writer.Write(saveString);
    }

    var wireLayoutSavePath =
        SaveSystem.GetPathToWireSaveFile(chipEditor.chipName);
    using (var writer = new StreamWriter(wireLayoutSavePath)) {
      writer.Write(wiringSaveString);
    }

    // Update parent chips using this chip
    var currentChipName = chipEditor.chipName;
    var savedChips = SaveSystem.GetAllSavedChips();
    foreach (var savedChip in savedChips)
      if (savedChip.componentNameList.Contains(currentChipName)) {
        var currentChipIndex = Array.FindIndex(
            savedChip.savedComponentChips, c => c.chipName == currentChipName);
        var updatedComponentChip = new SavedComponentChip(chipSaveData, chip);
        var oldComponentChip = savedChip.savedComponentChips[currentChipIndex];

        // Update component chip I/O
        foreach (var savedInputPin in updatedComponentChip.inputPins)
          foreach (var inputPin in oldComponentChip.inputPins) {
            if (savedInputPin.name != inputPin.name)
              continue;

            savedInputPin.parentChipIndex = inputPin.parentChipIndex;
            savedInputPin.parentChipOutputIndex =
                inputPin.parentChipOutputIndex;
            savedInputPin.isCylic = inputPin.isCylic;
          }

        // Write to file
        var parentSaveString = JsonUtility.ToJson(savedChip, UsePrettyPrint);
        var parentSavePath = SaveSystem.GetPathToSaveFile(savedChip.name);
        using var writer = new StreamWriter(parentSavePath);
        writer.Write(parentSaveString);
      }
  }

  public static void EditSavedChip(SavedChip savedChip,
                                   ChipSaveData chipSaveData) {}

  public static bool IsSafeToDelete(string chipName) {
    string[] notValidArray = { "AND",
                               "NOT",
                               "OR",
                               "XOR",
                               "HDD",
                               "4 BIT ENCODER",
                               "4 BIT DECODER",
                               "8 BIT ENCODER",
                               "8 BIT DECODER",
                               "16 BIT ENCODER",
                               "16 BIT DECODER" };
    if (notValidArray.Any(chipName.Contains))
      return false;

    var savedChips = SaveSystem.GetAllSavedChips();
    return savedChips.All(savedChip =>
                              !savedChip.componentNameList.Contains(chipName));
  }

  public static bool IsSignalSafeToDelete(string chipName, string signalName) {
    var savedChips = SaveSystem.GetAllSavedChips();
    foreach (var savedChip in savedChips) {
      if (!savedChip.componentNameList.Contains(chipName))
        continue;

      var parentChip = savedChip;
      var currentChipIndex = Array.FindIndex(parentChip.savedComponentChips,
                                             scc => scc.chipName == chipName);
      var currentChip = parentChip.savedComponentChips[currentChipIndex];
      var currentSignalIndex = Array.FindIndex(currentChip.outputPins,
                                               name => name.name == signalName);

      if (Array.Find(currentChip.inputPins, pin => pin.name == signalName &&
                                                   pin.parentChipIndex >= 0) !=
          null)
        return false;
      if (currentSignalIndex >= 0 &&
          parentChip.savedComponentChips.Any(
              scc => scc.inputPins.Any(
                  pin => pin.parentChipIndex == currentChipIndex &&
                         pin.parentChipOutputIndex == currentSignalIndex)))
        return false;
    }

    return true;
  }

  public static void Delete(string chipName) {
    File.Delete(SaveSystem.GetPathToSaveFile(chipName));
    File.Delete(SaveSystem.GetPathToWireSaveFile(chipName));
  }

  public static void Rename(string oldChipName, string newChipName) {
    if (oldChipName == newChipName)
      return;
    var savedChips = SaveSystem.GetAllSavedChips();
    foreach (var savedChip in savedChips) {
      var changed = false;
      if (savedChip.name == oldChipName) {
        savedChip.name = newChipName;
        changed = true;
      }

      for (var j = 0; j < savedChip.componentNameList.Length; j++) {
        var componentName = savedChip.componentNameList[j];
        if (componentName != oldChipName)
          continue;

        savedChip.componentNameList[j] = newChipName;
        changed = true;
      }

      foreach (var savedComponentChip in savedChip.savedComponentChips) {
        var componentChipName = savedComponentChip.chipName;
        if (componentChipName == oldChipName) {
          savedComponentChip.chipName = newChipName;
          changed = true;
        }
      }

      if (!changed)
        continue;

      var saveString = JsonUtility.ToJson(savedChip, UsePrettyPrint);
      // Write to file
      var savePath = SaveSystem.GetPathToSaveFile(savedChip.name);
      using var writer = new StreamWriter(savePath);
      writer.Write(saveString);
    }

    // Rename wire layer file
    var oldWireSaveFile = SaveSystem.GetPathToWireSaveFile(oldChipName);
    var newWireSaveFile = SaveSystem.GetPathToWireSaveFile(newChipName);
    try {
      File.Move(oldWireSaveFile, newWireSaveFile);
    } catch (Exception e) {
      Debug.LogError(e);
    }

    // Delete old chip save file
    File.Delete(SaveSystem.GetPathToSaveFile(oldChipName));
  }
}
}