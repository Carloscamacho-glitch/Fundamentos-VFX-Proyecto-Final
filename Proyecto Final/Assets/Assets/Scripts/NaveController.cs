using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private GameObject naveRota; // Nave dañada
    [SerializeField] private GameObject nave;     // Nave reparada
    [SerializeField] private float rangoDeteccion; // Distancia para activar reparación
    [SerializeField] private float delayCambioNave = 1.5f;           // Retraso antes de cambiar la nave
    [SerializeField] private bool naveReparada = false;                             // Estado de la nave
    [SerializeField] private bool reparacionEnProgreso = false;                     // Controla el delay
    [SerializeField] private float temporizadorReparacion = 0f;                     

    [SerializeField] private Transform jugador;

    [SerializeField] private AlertaMateriales alertaMateriales;
    [SerializeField] private AlertaNaveReparada alertaNaveReparada;
    [SerializeField] private float avisoTimer = 0f;

    [Header("Efectos")]
    [SerializeField] private ParticleSystem polvoReparacion; // Sistema de partículas

    void Update()
    {
        if (jugador == null) return;

        PlayerController player = jugador.GetComponent<PlayerController>();

        // Calcular distancia al jugador
        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Si está dentro del rango de detección
        if (distancia <= rangoDeteccion)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (player.TieneMotor() && player.GetMateriales() >= 2 && !naveReparada)
                {
                    IniciarReparacion();
                }
                else if (naveReparada)
                {
                    return;
                }
                else
                {
                    MostrarAvisoFaltanMateriales();
                }
            }
            
            // Si la reparación está en proceso, cuenta el tiempo
            if (reparacionEnProgreso)
            {
                temporizadorReparacion += Time.deltaTime;

                // Después de 1 segundo cambia la nave
                if (temporizadorReparacion >= delayCambioNave)
                {
                    FinalizarReparacion();
                }
            }
        }
        
        // Ocultar el aviso tras un tiempo
        if (avisoTimer > 0)
        {
            avisoTimer -= Time.deltaTime;
            if (avisoTimer <= 0 && alertaMateriales != null)
                alertaMateriales.MostrarAvisoFaltanMateriales(false);
            if (avisoTimer <= 0 && alertaNaveReparada != null)
                alertaNaveReparada.MostrarAvisoNaveReparada(false);
        }
    }

    // === MÉTODOS DE REPARACIÓN ===
    private void IniciarReparacion()
    {
        reparacionEnProgreso = true;
        temporizadorReparacion = 0f;

        // Activa partículas
        if (polvoReparacion != null)
            polvoReparacion.gameObject.SetActive(true);
    }

    private void FinalizarReparacion()
    {
        reparacionEnProgreso = false;
        naveReparada = true;

        // Cambia los modelos
        naveRota.SetActive(false);
        nave.SetActive(true);

        // Muestra aviso
        MostrarAvisoNaveReparada();
    }
    
    private void MostrarAvisoNaveReparada()
    {
        if (alertaNaveReparada != null)
        {
            alertaNaveReparada.MostrarAvisoNaveReparada(true);
            avisoTimer = 3f; // se mostrará durante 3 segundos
        }
    }

    private void MostrarAvisoFaltanMateriales()
    {
        if (alertaMateriales != null)
        {
            alertaMateriales.MostrarAvisoFaltanMateriales(true);
            avisoTimer = 3f; // se mostrará durante 3 segundos
        }
    }
}
