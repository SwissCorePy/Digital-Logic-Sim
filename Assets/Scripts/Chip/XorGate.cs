namespace Chip
{
    public class XorGate : BuiltinChip
    {
        protected override void ProcessOutput()
        {
            var outputSignal = inputPins[0].State ^ inputPins[1].State;
            outputPins[0].ReceiveSignal(outputSignal);
        }
    }
}