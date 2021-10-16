using Graphics;
using UnityEngine;
using Utility;

namespace Chip.Test {
  public class Bus : Chip {
    private const int HighZ = -1;

    public MeshRenderer meshRenderer;
    public Palette palette;

    protected override void ProcessOutput() {
      var outputSignal = -1;
      foreach (var pin in inputPins) {
        if (!pin.HasParent)
          continue;
        if (pin.State == HighZ)
          continue;
        outputSignal = pin.State == 1 ? 1 : 0;
      }

      foreach (var pin in outputPins)
        pin.ReceiveSignal(outputSignal);

      SetCol(outputSignal);
    }

    private void SetCol(int signal) {
      meshRenderer.material.color =
          signal == 1 ? palette.onCol : palette.offCol;
      if (signal == -1)
        meshRenderer.material.color = palette.highZCol;
    }

    public Pin GetBusConnectionPin(Pin wireStartPin, Vector2 connectionPos) {
      Pin connectionPin = null;
      // Wire wants to put data onto bus
      if (wireStartPin != null &&
          wireStartPin.pinType == Pin.PinType.ChipOutput)
        connectionPin = FindUnusedInputPin();
      else // Wire wants to get data from bus
        connectionPin = FindUnusedOutputPin();
      var lineCentre = (Vector2)transform.position;
      var pos = MathUtility.ClosestPointOnLineSegment(
          lineCentre + Vector2.left * 100, lineCentre + Vector2.right * 100,
          connectionPos);
      connectionPin.transform.position = pos;
      return connectionPin;
    }

    private Pin FindUnusedOutputPin() {
      foreach (var pin in outputPins)
        if (pin.childPins.Count == 0)
          return pin;
      Debug.Log("Ran out of pins");
      return null;
    }

    private Pin FindUnusedInputPin() {
      foreach (var pin in inputPins)
        if (pin.parentPin == null)
          return pin;
      Debug.Log("Ran out of pins");
      return null;
    }
  }
}
