using System;
using Chip;

namespace Save_System.Serializable {
[Serializable]
public class SavedOutputPin {
    public string name;
    public Pin.WireType wireType;

    public SavedOutputPin(ChipSaveData chipSaveData, Pin pin) {
        name = pin.pinName;
        wireType = pin.wireType;
    }
}
}
