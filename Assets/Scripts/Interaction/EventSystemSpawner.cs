using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
public class EventSystemSpawner : MonoBehaviour
{
    private void Start()
    {
        EventSystem sceneEventSystem = FindObjectOfType<EventSystem>();
        if (sceneEventSystem != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

}
}
