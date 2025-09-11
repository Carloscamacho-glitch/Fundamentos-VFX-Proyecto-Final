using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Se bloquea el mause al centro de la pantalla de juego y se hace invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
