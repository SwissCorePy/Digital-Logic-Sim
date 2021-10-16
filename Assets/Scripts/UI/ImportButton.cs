using Core;
using Save_System;
using StandaloneFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class ImportButton : MonoBehaviour {
    public Button importButton;
    public Manager manager;
    public ChipBarUI chipBarUI;

    private void Start() {
        importButton.onClick.AddListener(ImportChip);
    }

    private void ImportChip() {
        var extensions = new[] { new ExtensionFilter("Chip design", "dls") };

        StandaloneFileBrowser.StandaloneFileBrowser.OpenFilePanelAsync(
        "Import chip design", "", extensions, true, paths => {
            if (paths[0] != null && paths[0] != "") {
                ChipLoader.Import(paths[0]);
                EditChipBar();
            }
        });
    }

    private void EditChipBar() {
        chipBarUI.ReloadBar();
        SaveSystem.LoadAll(manager);
    }
}
}
