using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f; // Velocidad de rotación en grados/segundo

    void Update()
    {
        // Gira el skybox alrededor del eje Y
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
