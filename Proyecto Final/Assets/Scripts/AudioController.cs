using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource audioSource;   // Si no se asigna, se usa el del mismo GameObject
    [SerializeField] private AudioClip[] clips;         // Lista de clips de audio

    [Header("Opciones de reproducción")]
    [SerializeField] private bool playOnStart = true;   // Reproducir automáticamente al iniciar
    [SerializeField] private bool avoidRepeat = true;   // Evitar repetir el mismo clip consecutivo

    [Header("Tiempo entre pistas")]
    [SerializeField] private float tiempoMinimo = 30f;   // Mínimo tiempo de espera entre clips
    [SerializeField] private float tiempoMaximo = 60f;  // Máximo tiempo de espera entre clips

    private int lastIndex = -1;
    private bool initialized = false;
    private bool waiting = false;
    private float waitTimer = 0f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        initialized = audioSource != null && clips != null && clips.Length > 0;
        if (!initialized)
            Debug.LogWarning("PlayThenRandom: falta AudioSource o no hay clips asignados.");
    }

    private void Start()
    {
        if (playOnStart && initialized)
            PlayRandomClip();
    }

    private void Update()
    {
        if (!initialized) return;

        // Si estamos esperando, contamos el tiempo
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                PlayRandomClip();
            }
            return;
        }

        // Si no se está reproduciendo y no estamos esperando → iniciar espera
        if (!audioSource.isPlaying && audioSource.clip != null && !waiting)
        {
            waitTimer = Random.Range(tiempoMinimo, tiempoMaximo);
            waiting = true;
        }
    }

    private void PlayRandomClip()
    {
        if (clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);

        // Evitar repetir el mismo clip si es posible
        if (avoidRepeat && clips.Length > 1)
        {
            int attempts = 0;
            while (index == lastIndex && attempts < 10)
            {
                index = Random.Range(0, clips.Length);
                attempts++;
            }
        }

        lastIndex = index;
        audioSource.clip = clips[index];
        audioSource.Play();
    }

    // Método público para iniciar manualmente
    public void StartPlayback()
    {
        if (!initialized) return;
        PlayRandomClip();
    }
}