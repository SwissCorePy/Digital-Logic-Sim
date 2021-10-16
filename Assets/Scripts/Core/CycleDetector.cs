using System;
using System.Collections.Generic;
using Graphics;

namespace Core {
public static class CycleDetector {
  private static bool _currentChipHasCycle;

  public static void MarkAllCycles(ChipEditor chipEditor) {
    var chipsWithCycles = new List<Chip.Chip>();
    if (chipsWithCycles == null)
      throw new ArgumentNullException(nameof(chipsWithCycles));

    var examinedChips = new HashSet<Chip.Chip>();
    var chips = chipEditor.chipInteraction.allChips.ToArray();

    // Clear all cycle markings
    foreach (var chip in chips)
      foreach (var inputPin in chip.inputPins)
        inputPin.cyclic = false;
    // Mark cycles
    foreach (var chip in chips) {
      examinedChips.Clear();
      _currentChipHasCycle = false;
      MarkCycles(chip, chip, examinedChips);
      if (_currentChipHasCycle)
        chipsWithCycles.Add(chip);
    }
  }

  private static void MarkCycles(Chip.Chip originalChip, Chip.Chip currentChip,
                                 HashSet<Chip.Chip> examinedChips) {
    if (!examinedChips.Contains(currentChip))
      examinedChips.Add(currentChip);
    else
      return;

    foreach (var outputPin in currentChip.outputPins)
      foreach (var childPin in outputPin.childPins) {
        var childChip = childPin.chip;
        if (childChip == null)
          continue;

        if (childChip == originalChip) {
          _currentChipHasCycle = true;
          childPin.cyclic = true;
        }
        // Don't continue down this path if the pin has already been marked as
        // cyclic (doing so would lead to multiple pins along the cycle path
        // being marked, when only the first pin responsible for the cycle
        // should be)
        else if (!childPin.cyclic) {
          MarkCycles(originalChip, childChip, examinedChips);
        }
      }
  }
}
}
