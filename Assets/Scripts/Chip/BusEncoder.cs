using System.Linq;

namespace Chip {
public class BusEncoder : BuiltinChip {
  protected override void ProcessOutput() {
    var outputSignal = 0;
    foreach (var inputState in inputPins.Select(x => x.State)) {
      outputSignal <<= 1;
      outputSignal |= inputState;
    }

    outputPins[0].ReceiveSignal(outputSignal);
  }
}
}
