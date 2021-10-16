using System.Collections.Generic;
using Chip;
using Graphics;
using Interaction;
using UnityEngine;

namespace UI {
public class PinNameDisplayManager : MonoBehaviour {
  public PinNameDisplay pinNamePrefab;
  private ChipEditor _chipEditor;
  private ChipEditorOptions _editorDisplayOptions;
  private Pin _highlightedPin;

  private List<PinNameDisplay> _pinNameDisplays;
  private List<Pin> _pinsToDisplay;

  private void Awake() {
    _chipEditor = FindObjectOfType<ChipEditor>();
    _editorDisplayOptions = FindObjectOfType<ChipEditorOptions>();
    _chipEditor.pinAndWireInteraction.onMouseOverPin += OnMouseOverPin;
    _chipEditor.pinAndWireInteraction.onMouseExitPin += OnMouseExitPin;

    _pinNameDisplays = new List<PinNameDisplay>();
    _pinsToDisplay = new List<Pin>();
  }

  private void LateUpdate() {
    var mode = _editorDisplayOptions.activePinNameDisplayMode;
    _pinsToDisplay.Clear();

    if (mode == ChipEditorOptions.PinNameDisplayMode.AlwaysMain ||
        mode == ChipEditorOptions.PinNameDisplayMode.AlwaysAll) {
      if (mode == ChipEditorOptions.PinNameDisplayMode.AlwaysAll)
        foreach (var chip in _chipEditor.chipInteraction.allChips) {
          _pinsToDisplay.AddRange(chip.inputPins);
          _pinsToDisplay.AddRange(chip.outputPins);
        }

      foreach (var chip in _chipEditor.inputsEditor.signals)
        if (!_chipEditor.inputsEditor.selectedSignals.Contains(chip))
          _pinsToDisplay.AddRange(chip.outputPins);
      foreach (var chip in _chipEditor.outputsEditor.signals)
        if (!_chipEditor.outputsEditor.selectedSignals.Contains(chip))
          _pinsToDisplay.AddRange(chip.inputPins);
    }

    if (_highlightedPin) {
      var nameDisplayKey =
          InputHelper.AnyOfTheseKeysHeld(KeyCode.LeftAlt, KeyCode.RightAlt);
      if (nameDisplayKey || mode == ChipEditorOptions.PinNameDisplayMode.Hover)
        _pinsToDisplay.Add(_highlightedPin);
    }

    DisplayPinName(_pinsToDisplay);
  }

  private void DisplayPinName(List<Pin> pins) {
    if (_pinNameDisplays.Count < pins.Count) {
      var numToAdd = pins.Count - _pinNameDisplays.Count;
      for (var i = 0; i < numToAdd; i++)
        _pinNameDisplays.Add(Instantiate(pinNamePrefab, transform));
    } else if (_pinNameDisplays.Count > pins.Count) {
      for (var i = pins.Count; i < _pinNameDisplays.Count; i++)
        _pinNameDisplays[i].gameObject.SetActive(false);
    }

    for (var i = 0; i < pins.Count; i++) {
      _pinNameDisplays[i].gameObject.SetActive(true);
      _pinNameDisplays[i].Set(pins[i]);
    }
  }

  private void OnMouseOverPin(Pin pin) { _highlightedPin = pin; }

  private void OnMouseExitPin(Pin pin) {
    if (_highlightedPin == pin)
      _highlightedPin = null;
  }
}
}