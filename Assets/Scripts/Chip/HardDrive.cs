using System.Collections.Generic;

namespace Chip {
public class HardDrive : BuiltinChip {
    private static readonly Dictionary<string, List<int>> Contents =
        new Dictionary<string, List<int>>();
    private string _binary;

    protected override void ProcessOutput() {
        switch (inputPins[0].State) {
        case 0:
            var binary = "";
            for (var i = 1; i < 5; i++)
                binary += inputPins[i].State.ToString();
            if (Contents.ContainsKey(binary))
                for (var i = 0; i < outputPins.Length; i++)
                    outputPins[i].ReceiveSignal(Contents[binary][i]);
            else
                foreach (var _ in outputPins)
                    outputPins[0].ReceiveSignal(0);
            break;
        case 1:
            var address = "";
            var store = new List<int>();
            for (var i = 5; i < 13; i++)
                store.Add(inputPins[i].State);
            for (var i = 1; i < 5; i++)
                address += inputPins[i].State;
            if (Contents.ContainsKey(address))
                Contents.Remove(address);
            Contents.Add(address, store);
            break;
        default:
            foreach (var i in outputPins)
                i.ReceiveSignal(0);
            break;
        }
    }
}
}
