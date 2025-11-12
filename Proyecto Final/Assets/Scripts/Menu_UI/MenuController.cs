using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject creditosPanel;

    [Header("Objetos del juego")]
    [SerializeField] private GameObject planet;
    [SerializeField] private GameObject spawn;
    [SerializeField] private GameObject enemies;
    [SerializeField] private GameObject lucesModelos;
    [SerializeField] private GameObject luces;
    [SerializeField] private GameObject globalVolume;
    [SerializeField] private GameObject playerController;
    [SerializeField] private GameObject sol;
    private bool gameStarted = false;
    private bool creditos = false;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        creditosPanel.SetActive(false);

        planet.SetActive(false);
        spawn.SetActive(false);
        enemies.SetActive(false);
        lucesModelos.SetActive(false);
        luces.SetActive(false);
        globalVolume.SetActive(false);
        playerController.SetActive(false);
        sol.SetActive(false);

        Time.timeScale = 0f;
        ShowCursor(true);
    }

    void Update()
    {
        if (!gameStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape) && !creditos)
        {
            Pause();
            ShowCursor(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && creditos)
        {
            ReturnToMain();
        }
    }

    // Menu principal
    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        hudPanel.SetActive(true);

        planet.SetActive(true);
        spawn.SetActive(true);
        enemies.SetActive(true);
        lucesModelos.SetActive(true);
        luces.SetActive(true);
        globalVolume.SetActive(true);
        playerController.SetActive(true);
        sol.SetActive(true);

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

    public void Creditos()
    {
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        creditosPanel.SetActive(true);

        planet.SetActive(false);
        spawn.SetActive(false);
        enemies.SetActive(false);
        lucesModelos.SetActive(false);
        luces.SetActive(false);
        globalVolume.SetActive(false);
        playerController.SetActive(false);
        sol.SetActive(false);

        ShowCursor(true);
        creditos = true;

        Time.timeScale = 0f; // pausa total del tiempo
    }
}
