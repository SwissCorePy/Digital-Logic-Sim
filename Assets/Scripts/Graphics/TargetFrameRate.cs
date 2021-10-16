using UnityEngine;

namespace Graphics
{
public class TargetFrameRate : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}
}