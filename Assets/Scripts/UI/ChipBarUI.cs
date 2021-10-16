using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class ChipBarUI : MonoBehaviour {
  public RectTransform bar;
  public Transform buttonHolder;
  public CustomButton buttonPrefab;
  public float buttonSpacing = 15f;
  public float buttonWidthPadding = 10;
  public List<string> hideList;
  public Scrollbar horizontalScroll;

  public List<CustomButton> customButton = new List<CustomButton>();
  private Manager _manager;

  private void Awake() {
    _manager = FindObjectOfType<Manager>();
    _manager.CustomChipCreated += AddChipButton;
    _manager.CustomChipUpdated += UpdateChipButton;
    ReloadBar();
  }

  private void LateUpdate() { UpdateBarPos(); }

  public void ReloadBar() {
    foreach (var button in customButton)
      Destroy(button.gameObject);
    customButton.Clear();

    foreach (var chip in _manager.builtinChips)
      if (chip.chipName == "AND" || chip.chipName == "NOT" ||
          MainMenu.advancedChipsEnabled == 1)
        AddChipButton(chip);
    Canvas.ForceUpdateCanvases();
  }

  private void UpdateBarPos() {
    float barPosY = horizontalScroll.gameObject.activeSelf ? 16 : 0;
    bar.localPosition = new Vector3(0, barPosY, 0);
  }

  private void AddChipButton(Chip.Chip chip) {
    if (hideList.Contains(chip.chipName)) // Debug.Log("Hiding")
      return;
    var button = Instantiate(buttonPrefab);
    button.gameObject.name = "Create (" + chip.chipName + ")";
    // Set button text
    var buttonTextUI = button.GetComponentInChildren<TMP_Text>();
    buttonTextUI.text = chip.chipName;

    // Set button size
    var buttonRect = button.GetComponent<RectTransform>();
    buttonRect.sizeDelta =
        new Vector2(buttonTextUI.preferredWidth + buttonWidthPadding,
                    buttonRect.sizeDelta.y);

    // Set button position
    buttonRect.SetParent(buttonHolder, false);
    // buttonRect.localPosition = new Vector3 (rightmostButtonEdgeX +
    // buttonSpacing + buttonRect.sizeDelta.x / 2f, 0, 0);

    // Set button event
    // button.onClick.AddListener (() => manager.SpawnChip (chip));
    button.AddListener(() => _manager.SpawnChip(chip));

    customButton.Add(button);
  }

  private void UpdateChipButton(Chip.Chip chip) {
    if (hideList.Contains(chip.chipName)) // Debug.Log("Hiding")
      return;

    var button =
        customButton.Find(g => g.name == "Create (" + chip.chipName + ")");
    if (button != null) {
      button.ClearEvents();
      button.AddListener(() => _manager.SpawnChip(chip));
    }
  }
}
}