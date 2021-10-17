using System.Linq;
using TMPro;
using UnityEngine;

namespace UI {
public class InputFieldValidator : MonoBehaviour {
    public TMP_InputField inputField;
    public string validChars =
        "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]<>";

    private void Awake() {
        inputField.onValueChanged.AddListener(OnEdit);
    }

    private void OnValidate() {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
    }

    private void OnEdit(string newString) {
        var validString = newString.Where(t => validChars.Contains(t.ToString()))
                          .Aggregate("", (current, t) => current + t);

        inputField.SetTextWithoutNotify(validString);
    }
}
}
