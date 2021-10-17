﻿namespace Chip {
public class NotGate : BuiltinChip {
  protected override void ProcessOutput() {
    var outputSignal = 1 - inputPins[0].State;
    outputPins[0].ReceiveSignal(outputSignal);
  }
}
}
