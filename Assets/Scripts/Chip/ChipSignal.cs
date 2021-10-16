using Graphics;
using UnityEngine;

// Base class for input and output signals
namespace Chip
{
public class ChipSignal : Chip
{
    public int currentState;

    public Palette palette;
    public MeshRenderer indicatorRenderer;
    public MeshRenderer pinRenderer;
    public MeshRenderer wireRenderer;
    public Pin.WireType wireType = Pin.WireType.Simple;

    [HideInInspector] public string signalName;

    private bool _interactable = true;
    public bool displayGroupDecimalValue {
        get;
        set;
    }
    public bool useTwosComplement {
        get;
        set;
    } = true;

    public int GroupID {
        get;
        set;
    } = -1;

    public virtual void SetInteractable(bool interactable)
    {
        _interactable = interactable;

        if (interactable) return;

        indicatorRenderer.material.color = palette.nonInteractableCol;
        pinRenderer.material.color = palette.nonInteractableCol;
        wireRenderer.material.color = palette.nonInteractableCol;
    }

    public void SetDisplayState(int state)
    {
        if (indicatorRenderer && _interactable)
            indicatorRenderer.material.color = state == 1 ? palette.onCol : palette.offCol;
    }

    public static bool InSameGroup(ChipSignal signalA, ChipSignal signalB)
    {
        return signalA.GroupID == signalB.GroupID && signalA.GroupID != -1;
    }

    public virtual void UpdateSignalName(string newName)
    {
        signalName = newName;
    }
}
}