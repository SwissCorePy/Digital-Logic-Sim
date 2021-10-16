using System.Collections.Generic;
using Chip;
using Chip.Test;
using Graphics;
using Interaction;
using UnityEngine;

namespace Save_System {
public class SavedChipEditor : MonoBehaviour {
  public bool loadInEditMode;
  public string chipToEditName;
  public Wire wirePrefab;
  private GameObject _loadedChipHolder;

  public void Load(string chipName, GameObject chipHolder) {
    var loadedChip = Instantiate(chipHolder, transform, true);

    var topLevelChips =
        new List<Chip.Chip>(GetComponentsInChildren<Chip.Chip>());

    var subChips = GetComponentsInChildren<CustomChip>(true);
    foreach (var customChip in subChips) {
      if (customChip.transform.parent != loadedChip.transform)
        continue;

      customChip.gameObject.SetActive(true);
      topLevelChips.Add(customChip);
    }

    // topLevelChips.Sort ((a, b) => a.chipSaveIndex.CompareTo
    // (b.chipSaveIndex));

    var wiringSaveData =
        ChipLoader.LoadWiringFile(SaveSystem.GetPathToWireSaveFile(chipName));
    var wireIndex = 0;
    foreach (var savedWire in wiringSaveData.serializableWires) {
      var loadedWire = Instantiate(wirePrefab, loadedChip.transform);
      loadedWire.SetDepth(wireIndex);
      var parentPin = topLevelChips[savedWire.parentChipIndex]
                          .outputPins[savedWire.parentChipOutputIndex];
      var childPin = topLevelChips[savedWire.childChipIndex]
                         .inputPins[savedWire.childChipInputIndex];
      loadedWire.Connect(parentPin, childPin);
      loadedWire.SetAnchorPoints(savedWire.anchorPoints);
      FindObjectOfType<PinAndWireInteraction>().LoadWire(loadedWire);
      // player.AddWire (loadedWire);

      if (childPin.chip is Bus)
        childPin.transform.position =
            savedWire.anchorPoints[savedWire.anchorPoints.Length - 1];
      if (parentPin.chip is Bus)
        parentPin.transform.position = savedWire.anchorPoints[0];
      wireIndex++;
    }

    _loadedChipHolder = loadedChip;
  }

  public void CaptureLoadedChip(GameObject chipHolder) {
    if (!_loadedChipHolder)
      return;

    for (var i = _loadedChipHolder.transform.childCount - 1; i >= 0; i--)
      _loadedChipHolder.transform.GetChild(i).parent = chipHolder.transform;
    Destroy(_loadedChipHolder);
  }
}
}