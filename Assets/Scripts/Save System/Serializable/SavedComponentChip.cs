using System;

namespace Save_System.Serializable {
[Serializable]
public class SavedComponentChip {
    public string chipName;
    public double posX;
    public double posY;

    public SavedInputPin[] inputPins;
    public SavedOutputPin[] outputPins;

    public SavedComponentChip(ChipSaveData chipSaveData, Chip.Chip chip) {
        chipName = chip.chipName;

        // Store position in doubles and limit precision to reduce space in save
        // file
        const double precision = 10000;
        var position = chip.transform.position;
        posX = (int)(position.x * precision) / precision;
        posY = (int)(position.y * precision) / precision;

        // Input pins
        inputPins = new SavedInputPin[chip.inputPins.Length];
        for (var i = 0; i < inputPins.Length; i++)
            inputPins[i] = new SavedInputPin(chipSaveData, chip.inputPins[i]);

        // Output pins
        outputPins = new SavedOutputPin[chip.outputPins.Length];
        for (var i = 0; i < chip.outputPins.Length; i++)
            outputPins[i] = new SavedOutputPin(chipSaveData, chip.outputPins[i]);
    }
}
}
