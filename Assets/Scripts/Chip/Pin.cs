using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chip {
public class Pin : MonoBehaviour {
    public enum PinType { ChipInput, ChipOutput }

    public enum WireType { Simple, Bus4, Bus8, Bus16, Bus32 }

    public PinType pinType;

    public WireType wireType;

    // The chip that this pin is attached to (either as an input or output
    // terminal)
    public Chip chip;
    public string pinName;

    [HideInInspector]
    public bool cyclic;

    // Index of this pin in its associated chip's input or output pin array
    [HideInInspector]
    public int index;

    // The pin from which this pin receives its input signal
    // (multiple inputs not allowed in order to simplify simulation)
    [HideInInspector]
    public Pin parentPin;

    // The pins which this pin forwards its signal to
    [HideInInspector]
    public List<Pin> childPins = new List<Pin>();

    // Appearance
    private readonly Color _defaultCol = Color.black;

    private readonly Color _interactCol = new Color(0.7f, 0.7f, 0.7f);

    // Current state of the pin: 0 == LOW, 1 == HIGH
    private Material _material;

    public static float radius {
        get {
            var diameter = 0.215f;
            return diameter / 2;
        }
    }

    public static float interactionRadius => radius * 1.1f;

    // Get the current state of the pin: 0 == LOW, 1 == HIGH
    public int State {
        get;
        private set;
    }

    // Note that for ChipOutput pins, the chip itself is considered the parent, so
    // will always return true Otherwise, only true if the parentPin of this pin
    // has been set
    public bool HasParent => parentPin != null || pinType == PinType.ChipOutput;

    private void Awake() {
        _material = GetComponent<MeshRenderer>().material;
        _material.color = _defaultCol;
    }

    private void Start() {
        SetScale();
    }

    public void SetScale() {
        transform.localScale = Vector3.one * (radius * 2);
    }

    // Receive signal: 0 == LOW, 1 = HIGH
    // Sets the current state to the signal
    // Passes the signal on to any connected pins / electronic component
    public void ReceiveSignal(int signal) {
        State = signal;

        switch (pinType) {
        case PinType.ChipInput when !cyclic:
            chip.ReceiveInputSignal(this);
            break;
        case PinType.ChipOutput: {
            foreach (var pin in childPins)
                pin.ReceiveSignal(signal);

            break;
        }
        default:
            throw new ArgumentOutOfRangeException();
        }
    }

    public static void MakeConnection(Pin pinA, Pin pinB) {
        if (!IsValidConnection(pinA, pinB))
            return;

        var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
        var childPin = pinA.pinType == PinType.ChipInput ? pinA : pinB;

        parentPin.childPins.Add(childPin);
        childPin.parentPin = parentPin;
    }

    public static void RemoveConnection(Pin pinA, Pin pinB) {
        var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
        var childPin = pinA.pinType == PinType.ChipInput ? pinA : pinB;

        parentPin.childPins.Remove(childPin);
        childPin.parentPin = null;
    }

    public static bool IsValidConnection(Pin pinA, Pin pinB) {
        // Connection fails when pin wire types are different
        if (pinA.wireType != pinB.wireType)
            return false;
        // Connection is valid if one pin is an output pin, and the other is an
        // input pin
        return pinA.pinType != pinB.pinType;
    }

    public static bool TryConnect(Pin pinA, Pin pinB) {
        if (pinA.pinType == pinB.pinType)
            return false;

        var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
        var childPin = parentPin == pinB ? pinA : pinB;
        parentPin.childPins.Add(childPin);
        childPin.parentPin = parentPin;
        return true;
    }

    public void MouseEnter() {
        transform.localScale = Vector3.one * (interactionRadius * 2);
        _material.color = _interactCol;
    }

    public void MouseExit() {
        transform.localScale = Vector3.one * (radius * 2);
        _material.color = _defaultCol;
    }
}
}
