using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
public class CreateGroup : MonoBehaviour {
    public TMP_InputField groupSizeInput;
    public Button setSizeButton;
    public GameObject menuHolder;

    private int _groupSizeValue;
    private bool _menuActive;

    // Start is called before the first frame update
    private void Start() {
        _menuActive = false;
        _groupSizeValue = 8;
        setSizeButton.onClick.AddListener(SetGroupSize);
        groupSizeInput.onValueChanged.AddListener(SetCurrentText);
    }

    public event Action<int> ONGroupSizeSettingPressed;

    private void SetCurrentText(string groupSize) {
        _groupSizeValue = int.Parse(groupSize);
    }

    public void CloseMenu() {
        ONGroupSizeSettingPressed?.Invoke(_groupSizeValue);
        _menuActive = false;
        menuHolder.SetActive(false);
    }

    private void OpenMenu() {
        _menuActive = true;
        menuHolder.SetActive(true);
    }

    private void SetGroupSize() {
        if (_menuActive)
            CloseMenu();
        else
            OpenMenu();
    }
}
}
