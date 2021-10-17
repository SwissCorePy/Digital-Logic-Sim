using System.Collections.Generic;
using Chip;
using Interaction;
using TMPro;
using UnityEngine;

namespace UI {
public class DecimalDisplay : MonoBehaviour {
    public TMP_Text textPrefab;

    private List<SignalGroup> _displayGroups;
    private ChipInterfaceEditor _signalEditor;

    private void Start() {
        _displayGroups = new List<SignalGroup>();

        _signalEditor = GetComponent<ChipInterfaceEditor>();
        _signalEditor.onChipsAddedOrDeleted += RebuildGroups;
    }

    private void Update() {
        UpdateDisplay();
    }

    private void UpdateDisplay() {
        foreach (var signalGroup in _displayGroups)
            signalGroup.UpdateDisplay(_signalEditor);
    }

    private void RebuildGroups() {
        foreach (var signalGroup in _displayGroups)
            Destroy(signalGroup.text.gameObject);
        _displayGroups.Clear();

        var groups = _signalEditor.GetGroups();

        foreach (var group in groups) {
            if (!group[0].displayGroupDecimalValue)
                continue;

            var text = Instantiate(textPrefab, transform, true);
            _displayGroups.Add(new SignalGroup { signals = group, text = text });
        }

        UpdateDisplay();
    }

    private class SignalGroup {
        public ChipSignal[] signals;
        public TMP_Text text;

        public void UpdateDisplay(ChipInterfaceEditor editor) {
            if (editor.selectedSignals.Contains(signals[0])) {
                text.gameObject.SetActive(false);
            } else {
                text.gameObject.SetActive(true);
                var yPos = (signals[0].transform.position.y +
                            signals[signals.Length - 1].transform.position.y) /
                           2f;
                text.transform.position =
                    new Vector3(editor.transform.position.x, yPos, -0.5f);

                var useTwosComplement = signals[0].useTwosComplement;

                var decimalValue = 0;
                for (var i = 0; i < signals.Length; i++) {
                    var signalState = signals[signals.Length - 1 - i].currentState;
                    if (useTwosComplement && i == signals.Length - 1)
                        decimalValue |= -(signalState << i);
                    else
                        decimalValue |= signalState << i;
                }

                text.text = decimalValue + "";
            }
        }
    }
}
}
