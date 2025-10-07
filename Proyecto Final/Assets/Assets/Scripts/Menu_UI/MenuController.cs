using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    private bool gameStarted = false;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        Time.timeScale = 0f;
        ShowCursor(true);
    }

    void Update()
    {
        if (!gameStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
            ShowCursor(true);
        }
    }

    // Menu principal
    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);

        hudPanel.SetActive(true);

        Time.timeScale = 1f;
        ShowCursor(false);

        gameStarted = true;
    }

    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    // Menu de pausa
    private void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        hudPanel.SetActive(true);

        Time.timeScale = 1f; // reanudar juego

        ShowCursor(false);
    }

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);
        hudPanel.SetActive(false);

        Time.timeScale = 0f; // pausa total del tiempo
    }

    public void ReturnToMain()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // recargar escena actual
    }
}
