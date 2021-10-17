using System;
using System.Linq;
using Core;
using Interaction;
using Save_System;
using Save_System.Serializable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class EditChipMenu : MonoBehaviour {
    public TMP_InputField chipNameField;
    public Button doneButton;
    public Button deleteButton;
    public Button viewButton;
    public Button exportButton;
    public GameObject panel;
    public ChipBarUI chipBarUI;
    public bool isActive;
    private Chip.Chip _currentChip;
    private bool _focused;

    private bool _init;

    private Manager _manager;
    private string _nameBeforeChanging;

    public void Update() {
        if (!_focused)
            return;
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) &&
                !Input.GetMouseButtonDown(2))
            return;
        if (Camera.main is null)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider == null)
            return;

        if (hit.collider.name != panel.name)
            CloseEditChipMenu();
    }

    public void Init() {
        if (_init)
            return;

        chipBarUI = GameObject.Find("Chip Bar").GetComponent<ChipBarUI>();
        chipNameField.onValueChanged.AddListener(ChipNameFieldChanged);
        doneButton.onClick.AddListener(FinishCreation);
        deleteButton.onClick.AddListener(DeleteChip);
        viewButton.onClick.AddListener(ViewChip);
        exportButton.onClick.AddListener(ExportChip);
        _manager = FindObjectOfType<Manager>();
        FindObjectOfType<ChipInteraction>().editChipMenu = this;
        panel.gameObject.SetActive(false);
        _init = true;
        isActive = false;
    }

    public void EditChip(Chip.Chip chip) {
        panel.gameObject.SetActive(true);
        isActive = true;
        var chipUI = GameObject.Find("Create (" + chip.chipName + ")");
        var gameObject1 = gameObject;

        gameObject1.transform.position =
            chipUI.transform.position + new Vector3(7.5f, -0.65f, 0);
        var xVal = Math.Min(gameObject1.transform.position.x, 13.9f);
        xVal = Math.Max(xVal, -0.1f);

        var o = gameObject;
        var position = o.transform.position;

        position = new Vector3(xVal, position.y, position.z);
        o.transform.position = position;
        chipNameField.text = chip.chipName;
        _nameBeforeChanging = chip.chipName;
        doneButton.interactable = true;
        chipNameField.interactable = ChipSaver.IsSafeToDelete(_nameBeforeChanging);
        deleteButton.interactable = ChipSaver.IsSafeToDelete(_nameBeforeChanging);
        viewButton.interactable = chip.canBeEdited;
        exportButton.interactable = chip.canBeEdited;
        _focused = true;
        _currentChip = chip;
    }

    private void ChipNameFieldChanged(string value) {
        var formattedName = value.ToUpper();
        doneButton.interactable = IsValidChipName(formattedName.Trim());
        chipNameField.text = formattedName;
    }

    public bool IsValidRename(string chipName) {
        if (_nameBeforeChanging == chipName)
            // Name has not changed
            return true;
        if (!IsValidChipName(chipName))
            // Name is either empty, AND or NOT
            return false;
        var savedChips = SaveSystem.GetAllSavedChips();
        return savedChips.All(savedChip => savedChip.name != chipName);
    }

    private bool IsValidChipName(string chipName) {
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
                                   "16 BIT DECODER"
                                 };

        // If chipName is in notValidArray then is not a valid name
        return !notValidArray.Any(chipName.Contains) && chipName.Length != 0;
    }

    private void DeleteChip() {
        ChipSaver.Delete(_nameBeforeChanging);
        CloseEditChipMenu();
        EditChipBar();
    }

    private void EditChipBar() {
        chipBarUI.ReloadBar();
        SaveSystem.LoadAll(_manager);
    }

    private void FinishCreation() {
        if (chipNameField.text != _nameBeforeChanging) {
            // Chip has been renamed
            ChipSaver.Rename(_nameBeforeChanging, chipNameField.text.Trim());
            EditChipBar();
        }

        CloseEditChipMenu();
    }

    private void CloseEditChipMenu() {
        panel.gameObject.SetActive(false);
        isActive = false;
        _focused = false;
        _currentChip = null;
    }

    private void ViewChip() {
        if (_currentChip != null) {
            _manager.ViewChip(_currentChip);
            CloseEditChipMenu();
        }
    }

    private void ExportChip() {
        var path = StandaloneFileBrowser.StandaloneFileBrowser.SaveFilePanel(
                       "Export chip design", "", _currentChip.chipName + ".dls", "dls");
        if (path.Length != 0)
            ChipSaver.Export(_currentChip, path);
    }
}
}
