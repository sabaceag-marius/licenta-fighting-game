using UnityEngine;
public class FramerateCapper : MonoBehaviour
{    
    [SerializeField]
    private int targetFPS = 60;

    private void Awake()
    {
        Application.targetFrameRate = targetFPS;
    }
}