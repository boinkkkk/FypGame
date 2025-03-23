using UnityEngine;
using Cinemachine;

public class CinemachineHandler : MonoBehaviour
{
    private static CinemachineHandler instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep Cinemachine across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate cameras
        }
    }
}
