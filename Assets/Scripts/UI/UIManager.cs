using UnityEngine;

namespace UI {
public enum UIManagerState { Create, Update }

public class UIManager : MonoBehaviour {
  public GameObject createButton;
  public GameObject updateButton;

  private UIManagerState _state;

  public void Start() { UpdateState(); }

  public void ChangeState(UIManagerState newState) {
    if (_state != newState) {
      _state = newState;
      UpdateState();
    }
  }

  private void UpdateState() {
    switch (_state) {
    case UIManagerState.Create:
      createButton.SetActive(true);
      updateButton.SetActive(false);
      break;
    case UIManagerState.Update:
      createButton.SetActive(false);
      updateButton.SetActive(true);
      break;
    }
  }
}
}