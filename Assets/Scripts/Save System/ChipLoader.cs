using System;
using System.Collections.Generic;
using System.IO;
using Chip;
using Core;
using Graphics;
using Save_System.Serializable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Save_System
{
    public static class ChipLoader
    {
        public static SavedChip[] GetAllSavedChips(string[] chipPaths)
        {
            var savedChips = new SavedChip[chipPaths.Length];

            // Read saved chips from file
            for (var i = 0; i < chipPaths.Length; i++)
            {
                using var reader = new StreamReader(chipPaths[i]);
                var chipSaveString = reader.ReadToEnd();

                // If the save does not contain wireType and contains outputPinNames then its a previous version save (v 0.25 expected)
                if (!chipSaveString.Contains("wireType") || chipSaveString.Contains("outputPinNames") ||
                    !chipSaveString.Contains("outputPins"))
                {
                    // An update is made to the save string and returned
                    string updatedSave = SaveCompatibility.FixSaveCompatibility(chipSaveString);
                    savedChips[i] = JsonUtility.FromJson<SavedChip>(updatedSave);
                }
                else
                {
                    savedChips[i] = JsonUtility.FromJson<SavedChip>(chipSaveString);
                }
            }

            return savedChips;
        }

        public static void LoadAllChips(string[] chipPaths, Manager manager)
        {
            var savedChips = GetAllSavedChips(chipPaths);

            SortChipsByOrderOfCreation(ref savedChips);
            // Maintain dictionary of loaded chips (initially just the built-in chips)
            var loadedChips = new Dictionary<string, Chip.Chip>();
            foreach (var builtinChip in manager.builtinChips) loadedChips.Add(builtinChip.chipName, builtinChip);

            foreach (var chipToTryLoad in savedChips)
            {
                var loadedChipData = LoadChip(chipToTryLoad, loadedChips);
                var loadedChip = manager.LoadChip(loadedChipData);
                if (loadedChip is CustomChip custom) custom.ApplyWireModes();
                loadedChips.Add(loadedChip.chipName, loadedChip);
            }
        }

        // Instantiates all components that make up the given clip, and connects them up with wires
        // The components are parented under a single "holder" object, which is returned from the function
        private static ChipSaveData LoadChip(SavedChip chipToLoad, Dictionary<string, Chip.Chip> previouslyLoadedChips)
        {
            var loadedChipData = new ChipSaveData();
            var numComponents = chipToLoad.savedComponentChips.Length;
            loadedChipData.componentChips = new Chip.Chip[numComponents];
            loadedChipData.chipName = chipToLoad.name;
            loadedChipData.chipColour = chipToLoad.colour;
            loadedChipData.chipNameColour = chipToLoad.nameColour;
            loadedChipData.creationIndex = chipToLoad.creationIndex;

            // Spawn component chips (the chips used to create this chip)
            // These will have been loaded already, and stored in the previouslyLoadedChips dictionary
            for (var i = 0; i < numComponents; i++)
            {
                var componentToLoad = chipToLoad.savedComponentChips[i];
                var componentName = componentToLoad.chipName;
                var pos = new Vector2((float)componentToLoad.posX, (float)componentToLoad.posY);

                if (!previouslyLoadedChips.ContainsKey(componentName))
                    Debug.LogError("Failed to load sub component: " + componentName + " While loading " +
                                   chipToLoad.name);

                var loadedComponentChip =
                    Object.Instantiate(previouslyLoadedChips[componentName], pos, Quaternion.identity);
                loadedChipData.componentChips[i] = loadedComponentChip;

                // Load input pin names
                for (var inputIndex = 0;
                    inputIndex < componentToLoad.inputPins.Length &&
                    inputIndex < loadedChipData.componentChips[i].inputPins.Length;
                    inputIndex++)
                {
                    loadedChipData.componentChips[i].inputPins[inputIndex].pinName =
                        componentToLoad.inputPins[inputIndex].name;
                    loadedChipData.componentChips[i].inputPins[inputIndex].wireType =
                        componentToLoad.inputPins[inputIndex].wireType;
                }

                // Load output pin names
                for (var outputIndex = 0; outputIndex < componentToLoad.outputPins.Length; outputIndex++)
                {
                    loadedChipData.componentChips[i].outputPins[outputIndex].pinName =
                        componentToLoad.outputPins[outputIndex].name;
                    loadedChipData.componentChips[i].outputPins[outputIndex].wireType =
                        componentToLoad.outputPins[outputIndex].wireType;
                }
            }

            // Connect pins with wires
            for (var chipIndex = 0; chipIndex < chipToLoad.savedComponentChips.Length; chipIndex++)
            {
                var loadedComponentChip = loadedChipData.componentChips[chipIndex];
                for (var inputPinIndex = 0;
                    inputPinIndex < loadedComponentChip.inputPins.Length &&
                    inputPinIndex < chipToLoad.savedComponentChips[chipIndex].inputPins.Length;
                    inputPinIndex++)
                {
                    var savedPin = chipToLoad.savedComponentChips[chipIndex].inputPins[inputPinIndex];
                    var pin = loadedComponentChip.inputPins[inputPinIndex];

                    // If this pin should receive input from somewhere, then wire it up to that pin
                    if (savedPin.parentChipIndex != -1)
                    {
                        var connectedPin = loadedChipData.componentChips[savedPin.parentChipIndex]
                            .outputPins[savedPin.parentChipOutputIndex];
                        pin.cyclic = savedPin.isCylic;
                        Pin.TryConnect(connectedPin, pin);
                    }
                }
            }

            return loadedChipData;
        }

        private static ChipSaveData LoadChipWithWires(SavedChip chipToLoad,
            Dictionary<string, Chip.Chip> previouslyLoadedChips, Wire wirePrefab, ChipEditor chipEditor)
        {
            var loadedChipData = new ChipSaveData();
            var numComponents = chipToLoad.savedComponentChips.Length;
            loadedChipData.componentChips = new Chip.Chip[numComponents];
            loadedChipData.chipName = chipToLoad.name;
            loadedChipData.chipColour = chipToLoad.colour;
            loadedChipData.chipNameColour = chipToLoad.nameColour;
            loadedChipData.creationIndex = chipToLoad.creationIndex;
            var wiresToLoad = new List<Wire>();

            // Spawn component chips (the chips used to create this chip)
            // These will have been loaded already, and stored in the previouslyLoadedChips dictionary
            for (var i = 0; i < numComponents; i++)
            {
                var componentToLoad = chipToLoad.savedComponentChips[i];
                var componentName = componentToLoad.chipName;
                var pos = new Vector2((float)componentToLoad.posX, (float)componentToLoad.posY);

                if (!previouslyLoadedChips.ContainsKey(componentName))
                    Debug.LogError("Failed to load sub component: " + componentName + " While loading " +
                                   chipToLoad.name);

                var loadedComponentChip = Object.Instantiate(previouslyLoadedChips[componentName], pos,
                    Quaternion.identity, chipEditor.chipImplementationHolder);
                loadedComponentChip.gameObject.SetActive(true);
                loadedChipData.componentChips[i] = loadedComponentChip;

                // Load input pin names
                for (var inputIndex = 0;
                    inputIndex < componentToLoad.inputPins.Length &&
                    inputIndex < loadedChipData.componentChips[i].inputPins.Length;
                    inputIndex++)
                    loadedChipData.componentChips[i].inputPins[inputIndex].pinName =
                        componentToLoad.inputPins[inputIndex].name;

                // Load output pin names
                for (var outputIndex = 0;
                    outputIndex < componentToLoad.outputPins.Length &&
                    outputIndex < loadedChipData.componentChips[i].outputPins.Length;
                    outputIndex++)
                    loadedChipData.componentChips[i].outputPins[outputIndex].pinName =
                        componentToLoad.outputPins[outputIndex].name;
            }

            // Connect pins with wires
            for (var chipIndex = 0; chipIndex < chipToLoad.savedComponentChips.Length; chipIndex++)
            {
                var loadedComponentChip = loadedChipData.componentChips[chipIndex];
                for (var inputPinIndex = 0;
                    inputPinIndex < loadedComponentChip.inputPins.Length &&
                    inputPinIndex < chipToLoad.savedComponentChips[chipIndex].inputPins.Length;
                    inputPinIndex++)
                {
                    var savedPin = chipToLoad.savedComponentChips[chipIndex].inputPins[inputPinIndex];
                    var pin = loadedComponentChip.inputPins[inputPinIndex];

                    // If this pin should receive input from somewhere, then wire it up to that pin
                    if (savedPin.parentChipIndex != -1)
                    {
                        var connectedPin = loadedChipData.componentChips[savedPin.parentChipIndex]
                            .outputPins[savedPin.parentChipOutputIndex];
                        pin.cyclic = savedPin.isCylic;
                        if (Pin.TryConnect(connectedPin, pin))
                        {
                            var loadedWire = Object.Instantiate(wirePrefab, chipEditor.wireHolder);
                            loadedWire.Connect(connectedPin, pin);
                            wiresToLoad.Add(loadedWire);
                        }
                    }
                }
            }

            loadedChipData.wires = wiresToLoad.ToArray();

            return loadedChipData;
        }

        public static SavedWireLayout LoadWiringFile(string path)
        {
            using var reader = new StreamReader(path);
            var wiringSaveString = reader.ReadToEnd();
            return JsonUtility.FromJson<SavedWireLayout>(wiringSaveString);
        }

        private static void SortChipsByOrderOfCreation(ref SavedChip[] chips)
        {
            var sortedChips = new List<SavedChip>(chips);
            sortedChips.Sort((a, b) => a.creationIndex.CompareTo(b.creationIndex));
            chips = sortedChips.ToArray();
        }

        public static ChipSaveData GetChipSaveData(Chip.Chip chip, Chip.Chip[] builtinChips,
            List<Chip.Chip> spawnableChips, Wire wirePrefab, ChipEditor chipEditor)
        {
            // @NOTE: chipEditor can be removed here if:
            //     * Chip & wire instantiation is inside their respective implementation holders is inside the chipEditor
            //     * the wire connections are done inside ChipEditor.LoadFromSaveData instead of ChipLoader.LoadChipWithWires

            SavedChip chipToTryLoad;
            var savedChips = SaveSystem.GetAllSavedChips();

            using (var reader = new StreamReader(SaveSystem.GetPathToSaveFile(chip.name)))
            {
                var chipSaveString = reader.ReadToEnd();
                chipToTryLoad = JsonUtility.FromJson<SavedChip>(chipSaveString);
            }

            if (chipToTryLoad == null)
                return null;

            SortChipsByOrderOfCreation(ref savedChips);
            // Maintain dictionary of loaded chips (initially just the built-in chips)
            var loadedChips = new Dictionary<string, Chip.Chip>();
            foreach (var builtinChip in builtinChips) loadedChips.Add(builtinChip.chipName, builtinChip);
            foreach (var loadedChip in spawnableChips)
            {
                if (loadedChips.ContainsKey(loadedChip.chipName)) continue;
                loadedChips.Add(loadedChip.chipName, loadedChip);
            }

            var loadedChipData = LoadChipWithWires(chipToTryLoad, loadedChips, wirePrefab, chipEditor);
            var wireLayout = LoadWiringFile(SaveSystem.GetPathToWireSaveFile(loadedChipData.chipName));

            Debug.Log("Wires: " + wireLayout.serializableWires);

            // Set wires anchor points
            var cont = 0;
            foreach (var wire in wireLayout.serializableWires)
            {
                var startPinName = loadedChipData
                    .componentChips[wire.parentChipIndex]
                    .outputPins[wire.parentChipOutputIndex]
                    .pinName;

                cont++;
                Debug.Log(cont);

                var elementIndex = wire.childChipIndex;
                var lenght = loadedChipData.componentChips.Length;

                Debug.Log("-------\n1. Index: " + elementIndex + " for lenght " + lenght);

                var unused = loadedChipData.componentChips[wire.childChipIndex];

                var element2Index = wire.childChipInputIndex;
                var lenght2 = loadedChipData.componentChips[wire.childChipIndex].inputPins.Length;

                Debug.Log("2. Index: " + element2Index + " for lenght " + lenght2);

                var unused2 = loadedChipData
                    .componentChips[wire.childChipIndex]
                    .inputPins[wire.childChipInputIndex];

                var endPinName = loadedChipData
                    .componentChips[wire.childChipIndex]
                    .inputPins[wire.childChipInputIndex]
                    .pinName;

                var wireIndex = Array.FindIndex(loadedChipData.wires,
                    w => w.startPin.pinName == startPinName && w.endPin.pinName == endPinName);
                if (wireIndex >= 0) loadedChipData.wires[wireIndex].SetAnchorPoints(wire.anchorPoints);
            }

            return loadedChipData;
        }

        public static void Import(string path)
        {
            var allChips = SaveSystem.GetAllSavedChips();
            var nameUpdateLookupTable = new Dictionary<string, string>();

            using var reader = new StreamReader(path);
            var numberOfChips = int.Parse(reader.ReadLine() ?? throw new InvalidOperationException());

            for (var i = 0; i < numberOfChips; i++)
            {
                var chipName = reader.ReadLine();
                var saveDataLength = int.Parse(reader.ReadLine() ?? throw new InvalidOperationException());
                var wireSaveDataLength = int.Parse(reader.ReadLine() ?? throw new InvalidOperationException());

                var saveData = "";
                var wireSaveData = "";

                for (var j = 0; j < saveDataLength; j++) saveData += reader.ReadLine() + "\n";
                for (var j = 0; j < wireSaveDataLength; j++) wireSaveData += reader.ReadLine() + "\n";

                // Rename chip if already exist
                if (Array.FindIndex(allChips, c => c.name == chipName) >= 0)
                {
                    var nameCounter = 2;
                    string newName;
                    do
                    {
                        newName = chipName + nameCounter;
                        nameCounter++;
                    } while (Array.FindIndex(allChips, c => c.name == newName) >= 0);

                    nameUpdateLookupTable.Add(chipName ?? throw new InvalidOperationException(), newName);
                    chipName = newName;
                }

                // Update name inside file if there was some names changed
                foreach (var nameToReplace in nameUpdateLookupTable)
                    saveData = saveData.Replace(
                        "\"name\": \"" + nameToReplace.Key + "\"",
                        "\"name\": \"" + nameToReplace.Value + "\""
                    ).Replace(
                        "\"chipName\": \"" + nameToReplace.Key + "\"",
                        "\"chipName\": \"" + nameToReplace.Value + "\""
                    );

                var chipSaveFile = SaveSystem.GetPathToSaveFile(chipName);
                var chipWireSaveFile = SaveSystem.GetPathToWireSaveFile(chipName);

                using (var writer = new StreamWriter(chipSaveFile))
                {
                    writer.Write(saveData);
                }

                using (var writer = new StreamWriter(chipWireSaveFile))
                {
                    writer.Write(wireSaveData);
                }
            }
        }
    }
}