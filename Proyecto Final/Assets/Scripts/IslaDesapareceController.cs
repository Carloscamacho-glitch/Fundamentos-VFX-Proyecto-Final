using UnityEngine;
using System.Collections;

public class IslaDesapareceController : MonoBehaviour
{
    [Header("Configuración de aparición/desaparición")]
    [SerializeField] private float tiempoVisible = 60f;                 // Tiempo que la isla permanece visible
    [SerializeField] private float tiempoInvisible = 5f;          // Tiempo que la isla permanece invisible
    [SerializeField] private float rangoJugador = 50f;          // Rango alrededor del jugador para evitar desaparición
    [SerializeField] private float velocidadDesplazamiento = 2f;            // Velocidad de movimiento hacia el centro
    [SerializeField] private float velocidadEscala = 2f;             // Velocidad de escalado al reaparecer
    [SerializeField] private float velocidadRotacionX = 180f; // grados por segundo en desaparición

    [Header("Referencias")]
    [SerializeField] private GameObject islaDesaparece;         // Objeto de la isla que desaparece
    [SerializeField] private Transform centroPlaneta; // Punto hacia donde la isla se mueve al desaparecer

    private bool visible = true;             // Estado actual de visibilidad
    private bool enAnimacion = false;               // Indica si está en proceso de animación
    private float timer = 0f;           // Temporizador para controlar tiempos
    private Transform jugador;           // Referencia al jugador
    private Vector3 posicionInicial;                // Posición inicial de la isla
    private Vector3 escalaInicial;              // Escala inicial de la isla

    // Control de rotaciones aleatorias
    private float ultimaRotacionY = 0f;
    private int repeticionesRotacion = 0;
    private const int maxRepeticiones = 3;

    void Start()
    {
        if (islaDesaparece == null)
            islaDesaparece = transform.Find("IslaDesaparece")?.gameObject;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            jugador = playerObj.transform;

        if (centroPlaneta == null)
        {
            GameObject planeta = GameObject.Find("Planeta");
            if (planeta != null)
                centroPlaneta = planeta.transform;
        }

        if (islaDesaparece != null)
        {
            posicionInicial = islaDesaparece.transform.position;
            escalaInicial = islaDesaparece.transform.localScale;
            ultimaRotacionY = islaDesaparece.transform.eulerAngles.y;
        }

        SetVisible(true);
        timer = tiempoVisible;
    }

    void Update()
    {
        if (enAnimacion || jugador == null || islaDesaparece == null)
            return;

        if (EstaCercaDelJugador())
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (visible)
            {
                StartCoroutine(AnimarDesaparicion());
                timer = tiempoInvisible;
            }
            else
            {
                StartCoroutine(AnimarReaparicion());
                timer = tiempoVisible;
            }
        }
    }

    private IEnumerator AnimarDesaparicion()
    {
        enAnimacion = true;

        Vector3 destino = centroPlaneta != null ? centroPlaneta.position : Vector3.zero;
        float progreso = 0f;
        Vector3 inicio = islaDesaparece.transform.position;

        // Guardar rotación inicial
        Quaternion rotacionInicial = islaDesaparece.transform.rotation;

        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidadDesplazamiento;

            // Movimiento hacia el centro
            islaDesaparece.transform.position = Vector3.Lerp(inicio, destino, progreso);

            // Escala reduciéndose
            islaDesaparece.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, progreso);

            // Rotación en el eje X en sentido del reloj
            islaDesaparece.transform.Rotate(Vector3.right, velocidadRotacionX * Time.deltaTime, Space.Self);

            yield return null;
        }

        SetVisible(false);
        islaDesaparece.transform.rotation = rotacionInicial; // Restablecer rotación original
        enAnimacion = false;
    }

    private IEnumerator AnimarReaparicion()
    {
        enAnimacion = true;

        // Seleccionar rotación Y aleatoria con limitador
        float nuevaRotY = (Random.value < 0.5f) ? 0f : 180f;
        if (Mathf.Approximately(nuevaRotY, ultimaRotacionY))
        {
            repeticionesRotacion++;
            if (repeticionesRotacion >= maxRepeticiones)
            {
                nuevaRotY = (ultimaRotacionY == 0f) ? 180f : 0f;
                repeticionesRotacion = 0;
            }
        }
        else
        {
            repeticionesRotacion = 0;
        }

        ultimaRotacionY = nuevaRotY;
        Vector3 rot = islaDesaparece.transform.eulerAngles;
        rot.y = nuevaRotY;
        islaDesaparece.transform.eulerAngles = rot;

        SetVisible(true);
        islaDesaparece.transform.localScale = Vector3.zero;
        islaDesaparece.transform.position = posicionInicial;

        float progreso = 0f;
        while (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidadEscala;
            islaDesaparece.transform.localScale = Vector3.Lerp(Vector3.zero, escalaInicial, progreso);
            yield return null;
        }

        enAnimacion = false;
    }

    private bool EstaCercaDelJugador()
    {
        GameObject[] islas = GameObject.FindGameObjectsWithTag("IslaDesaparece");
        foreach (GameObject isla in islas)
        {
            float distancia = Vector3.Distance(jugador.position, isla.transform.position);
            if (distancia <= rangoJugador)
                return true;
        }
        return false;
    }

    private void SetVisible(bool estado)
    {
        visible = estado;
        if (islaDesaparece != null)
            islaDesaparece.SetActive(estado);
    }
}