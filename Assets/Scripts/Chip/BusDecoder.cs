using System.Linq;

namespace Chip
{
public class BusDecoder : BuiltinChip
{
    protected override void ProcessOutput()
    {
        var inputSignal = inputPins[0].State;
        foreach (var outputPin in outputPins.Reverse())
        {
            outputPin.ReceiveSignal(inputSignal & 1);
            inputSignal >>= 1;
        }
    }
}
}