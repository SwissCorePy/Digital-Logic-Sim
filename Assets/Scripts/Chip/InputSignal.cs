using System.Linq;

// Provides input signal (0 or 1) to a chip.
// When designing a chip, this input signal can be manually set to 0 or 1 by the player.
namespace Chip
{
    public class InputSignal : ChipSignal
    {
        protected override void Start()
        {
            base.Start();
            SetCol();
        }

        private void OnMouseDown()
        {
            // Allow only to click on single wires, not on bus wires
            if (outputPins.All(x => x.wireType == Pin.WireType.Simple))
                ToggleActive();
        }

        public void ToggleActive()
        {
            currentState = 1 - currentState;
            SetCol();
        }

        public void SendSignal(int signal)
        {
            currentState = signal;
            outputPins[0].ReceiveSignal(signal);
            SetCol();
        }

        public void SendOffSignal()
        {
            outputPins[0].ReceiveSignal(0);
            SetCol();
        }

        public void SendSignal()
        {
            outputPins[0].ReceiveSignal(currentState);
        }

        private void SetCol()
        {
            SetDisplayState(currentState);
        }

        public override void UpdateSignalName(string newName)
        {
            base.UpdateSignalName(newName);
            outputPins[0].pinName = newName;
        }
    }
}