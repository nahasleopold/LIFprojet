using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;

    private int currentCamera = 0;

    void Start()
    {
        ShowCamera(currentCamera);
    }

    void Update()
    {
        // Appuyer sur C pour passer à la caméra suivante
        if (Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            currentCamera++;

            if (currentCamera >= cameras.Length)
            {
                currentCamera = 0;
            }

            ShowCamera(currentCamera);
        }
    }

    private void ShowCamera(int cameraIndex)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == cameraIndex)
            {
                // La caméra choisie est affichée sur Display 1
                cameras[i].targetDisplay = 0;
            }
            else
            {
                // Les autres restent actives mais sont envoyées
                // vers d'autres Displays
                cameras[i].targetDisplay = i + 1;
            }
        }

        Debug.Log("Caméra affichée : " + cameras[cameraIndex].name);
    }
}