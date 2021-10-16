using Chip;
using Core;
using Interaction;
using Save_System;
using UnityEngine;

namespace Graphics
{
public class ChipEditor : MonoBehaviour
{
    public Transform chipImplementationHolder;
    public Transform wireHolder;
    public ChipInterfaceEditor inputsEditor;
    public ChipInterfaceEditor outputsEditor;
    public ChipInteraction chipInteraction;
    public PinAndWireInteraction pinAndWireInteraction;

    [HideInInspector] public string chipName;

    [HideInInspector] public Color chipColour;

    [HideInInspector] public Color chipNameColour;

    [HideInInspector] public int creationIndex;

    private void Awake()
    {
        InteractionHandler[] allHandlers = { inputsEditor, outputsEditor, chipInteraction, pinAndWireInteraction };
        foreach (var handler in allHandlers) handler.InitAllHandlers(allHandlers);

        pinAndWireInteraction.Init(chipInteraction, inputsEditor, outputsEditor);
        pinAndWireInteraction.onConnectionChanged += OnChipNetworkModified;
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }

    private void Start()
    {
        inputsEditor.CurrentEditor = this;
        outputsEditor.CurrentEditor = this;
    }

    private void LateUpdate()
    {
        inputsEditor.OrderedUpdate();
        outputsEditor.OrderedUpdate();
        pinAndWireInteraction.OrderedUpdate();
        chipInteraction.OrderedUpdate();
    }

    private void OnChipNetworkModified()
    {
        CycleDetector.MarkAllCycles(this);
    }

    public void LoadFromSaveData(ChipSaveData saveData)
    {
        chipName = saveData.chipName;
        chipColour = saveData.chipColour;
        chipNameColour = saveData.chipNameColour;
        creationIndex = saveData.creationIndex;

        // Load component chips
        foreach (var componentChip in saveData.componentChips)
            switch (componentChip)
            {
            case InputSignal inp:
                inp.wireType = inp.outputPins[0].wireType;
                inputsEditor.LoadSignal(inp);
                break;
            case OutputSignal outp:
                outp.wireType = outp.inputPins[0].wireType;
                outputsEditor.LoadSignal(outp);
                break;
            default:
                chipInteraction.LoadChip(componentChip);
                break;
            }

        // Load wires
        if (saveData.wires == null) return;

        foreach (var wire in saveData.wires) pinAndWireInteraction.LoadWire(wire);
    }
}
}