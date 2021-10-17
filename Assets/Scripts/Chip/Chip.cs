using Core;
using UnityEngine;

namespace Chip {
public class Chip : MonoBehaviour {
  public string chipName = "Untitled";
  public Pin[] inputPins;
  public Pin[] outputPins;
  public bool canBeEdited;

  // Cached components
  [HideInInspector]
  public BoxCollider2D bounds;

  private int _lastSimulatedFrame;
  private int _lastSimulationInitFrame;

  // Number of input signals received (on current simulation step)
  private int _numInputSignalsReceived;

  public Vector2 BoundsSize => bounds.size;

  protected virtual void Awake() { bounds = GetComponent<BoxCollider2D>(); }

  protected virtual void Start() { SetPinIndices(); }

  public void InitSimulationFrame() {
    if (_lastSimulationInitFrame == Simulation.simulationFrame)
      return;
    _lastSimulationInitFrame = Simulation.simulationFrame;
    ProcessCycleAndUnconnectedInputs();
  }

  // Receive input signal from pin: either pin has power, or pin does not have
  // power. Once signals from all input pins have been received, calls the
  // ProcessOutput() function.
  public virtual void ReceiveInputSignal(Pin pin) {
    // Reset if on new step of simulation
    if (_lastSimulatedFrame != Simulation.simulationFrame) {
      _lastSimulatedFrame = Simulation.simulationFrame;
      _numInputSignalsReceived = 0;
      InitSimulationFrame();
    }

    _numInputSignalsReceived++;

    if (_numInputSignalsReceived == inputPins.Length)
      ProcessOutput();
  }

  private void ProcessCycleAndUnconnectedInputs() {
    foreach (var inputPin in inputPins)
      if (inputPin.cyclic)
        ReceiveInputSignal(inputPin);
      else if (!inputPin.parentPin)
        inputPin.ReceiveSignal(0);
    // ReceiveInputSignal (inputPins[i]);
  }

  // Called once all inputs to the component are known.
  // Sends appropriate output signals to output pins
  protected virtual void ProcessOutput() {}

  private void SetPinIndices() {
    for (var i = 0; i < inputPins.Length; i++)
      inputPins[i].index = i;
    for (var i = 0; i < outputPins.Length; i++)
      outputPins[i].index = i;
  }
}
}
