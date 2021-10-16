using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
public class RunButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TMP_Text buttonText;
    public Color normalCol = Color.white;
    public Color nonInteractableCol = Color.grey;
    public Color highlightedCol = Color.white;
    public Simulation sim;
    private bool _highlighted;

    private void Update()
    {
        var col = _highlighted ? highlightedCol : normalCol;
        buttonText.color = button.interactable ? col : nonInteractableCol;

        buttonText.text = sim.active switch
    {
        true when buttonText.text != "STOP" => "STOP",
        false when buttonText.text != "RUN" => "RUN",
        _ => buttonText.text
    };
}

private void OnEnable()
    {
        _highlighted = false;
    }

    private void OnValidate()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonText == null) buttonText = transform.GetComponentInChildren<TMP_Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable) _highlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlighted = false;
    }
}
}