using Chip;
using Graphics;
using UnityEngine;

namespace Core
{
public class Simulation : MonoBehaviour
{
    private static Simulation _instance;
    public bool active;

    public float minStepTime = 0.075f;
    private ChipEditor _chipEditor;
    private InputSignal[] _inputSignals;
    private float _lastStepTime;

    public static int simulationFrame {
        get;
        private set;
    }

    private void Awake()
    {
        simulationFrame = 0;
    }


    private void Update()
    {
        // If simulation is off StepSimulation is not executed.
        if (Time.time - _lastStepTime > minStepTime && active)
        {
            _lastStepTime = Time.time;
            simulationFrame++;
            StepSimulation();
        }
    }

    public void ToggleActive()
    {
        // Method called by the "Run/Stop" button that toggles simulation active/inactive
        active = !active;

        simulationFrame++;
        if (active)
            ResumeSimulation();
        else
            StopSimulation();
    }

    private void ClearOutputSignals()
    {
        var outputSignals = _chipEditor.outputsEditor.signals;
        foreach (var chipSignal in outputSignals)
        {
            chipSignal.SetDisplayState(0);
            chipSignal.currentState = 0;
        }
    }

    private void ProcessInputs()
    {
        var inputSignals = _chipEditor.inputsEditor.signals;
        foreach (var chipSignal in inputSignals) ((InputSignal)chipSignal).SendSignal();
    }

    private void StopSimulation()
    {
        RefreshChipEditorReference();

        var allWires = _chipEditor.pinAndWireInteraction.allWires;
        foreach (var wire in allWires)
            // Tell all wires the simulation is inactive makes them all inactive (gray colored)
            wire.TellWireSimIsOff();

        // If sim is not active all output signals are set with a temporal value of 0
        // (group signed/unsigned displayed value) and get gray colored (turned off)
        ClearOutputSignals();
    }

    private void ResumeSimulation()
    {
        StepSimulation();

        var allWires = _chipEditor.pinAndWireInteraction.allWires;
        foreach (var wire in allWires)
            // Tell all wires the simulation is active makes them all active (dynamic colored based on the circuits logic)
            wire.tellWireSimIsOn();
    }

    private void StepSimulation()
    {
        RefreshChipEditorReference();
        ClearOutputSignals();
        InitChips();
        ProcessInputs();
    }

    private void InitChips()
    {
        var allChips = _chipEditor.chipInteraction.allChips;
        foreach (var chip in allChips) chip.InitSimulationFrame();
    }

    private void RefreshChipEditorReference()
    {
        if (_chipEditor == null) _chipEditor = FindObjectOfType<ChipEditor>();
    }
}
}