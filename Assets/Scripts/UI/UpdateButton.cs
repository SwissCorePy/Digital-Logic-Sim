using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
public class UpdateButton : MonoBehaviour
{
    public Button updateButton;

    public void Start()
    {
        updateButton.onClick.AddListener(ChipUpdatePressed);
    }

    public event Action ONChipUpdatePressed;

    private void ChipUpdatePressed()
    {
        ONChipUpdatePressed?.Invoke();
    }
}
}