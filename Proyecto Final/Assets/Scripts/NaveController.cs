using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private GameObject naveRota; // Nave dañada
    [SerializeField] private GameObject nave;     // Nave reparada
    [SerializeField] private float rangoDeteccion = 10f; // Distancia para activar reparación
    [SerializeField] private float delayCambioNave = 1f; // Retraso antes de cambiar la nave
    private bool naveReparada = false;    
    private bool reparacionEnProgreso = false; 
    private float temporizadorReparacion = 0f;

    [SerializeField] private Transform jugador;

    [SerializeField] private AlertaMateriales alertaMateriales;
    [SerializeField] private AlertaNaveReparada alertaNaveReparada;
    [SerializeField] private AlertaInteraccionNave alertaInteraccionNave;
    [SerializeField] private AlertaInteraccionNaveFinJuego alertaInteraccionNaveFinJuego;
    [SerializeField] private MenuController menuController;private float avisoTimer = 0f;

    [Header("Efectos")]
    [SerializeField] private ParticleSystem polvoReparacion;

    private bool jugadorEnRango = false;     // Detecta si el jugador está dentro del rango
    private bool avisoInteraccionMostrado = false; // Evita mostrar varias veces el aviso
    private bool avisoInteraccionFinJuegoMostrado = false; // Evita mostrar varias veces el aviso

    void Update()
    {
        if (jugador == null) return;

        PlayerController player = jugador.GetComponent<PlayerController>();
        float distancia = Vector3.Distance(transform.position, jugador.position);

        // --- Detección del jugador en rango ---
        if (distancia <= rangoDeteccion)
        {
            jugadorEnRango = true;

            // Mostrar aviso de interacción solo una vez al entrar en rango
            if (!naveReparada && !avisoInteraccionMostrado)
            {
                MostrarAvisoInteraccionNave();
                avisoInteraccionMostrado = true;
            }
            else if (naveReparada && !avisoInteraccionFinJuegoMostrado)
            {
                OcultarAvisoInteraccionNave();
                MostrarAvisoInteraccionNaveFinJuego();
                avisoInteraccionFinJuegoMostrado = true;
            }

            // Permitir reparación
            if (Input.GetKey(KeyCode.E))
            {
                if (player.TieneMotor() && player.GetMateriales() >= 2 && !naveReparada)
                {
                    IniciarReparacion();
                }
                else if (!naveReparada)
                {
                    MostrarAvisoFaltanMateriales();
                }
                else if (naveReparada)
                {
                    menuController.Creditos();
                }
            }

            if (reparacionEnProgreso)
            {
                temporizadorReparacion += Time.deltaTime;
                if (temporizadorReparacion >= delayCambioNave)
                    FinalizarReparacion();
            }
        }
        else
        {
            jugadorEnRango = false;
            avisoInteraccionMostrado = false; // Permite mostrar el aviso nuevamente si el jugador se aleja y regresa
            avisoInteraccionFinJuegoMostrado = false;
        }

        // Ocultar avisos tras su tiempo
        if (avisoTimer > 0)
        {
            avisoTimer -= Time.deltaTime;
            if (avisoTimer <= 0)
            {
                if (alertaMateriales != null)
                    alertaMateriales.MostrarAvisoFaltanMateriales(false);
                if (alertaNaveReparada != null)
                    alertaNaveReparada.MostrarAvisoNaveReparada(false);
                if (alertaInteraccionNave != null)
                    alertaInteraccionNave.MostrarAvisodeInteraccionNave(false);
                if (alertaInteraccionNaveFinJuego != null)
                    alertaInteraccionNaveFinJuego.MostrarAvisodeInteraccionNaveFinJuego(false);
            }
        }
    }

    // === MÉTODOS DE REPARACIÓN ===
    private void IniciarReparacion()
    {
        reparacionEnProgreso = true;
        temporizadorReparacion = 0f;

        if (polvoReparacion != null)
            polvoReparacion.gameObject.SetActive(true);
    }

    private void FinalizarReparacion()
    {
        reparacionEnProgreso = false;
        naveReparada = true;

        naveRota.SetActive(false);
        nave.SetActive(true);

        MostrarAvisoNaveReparada();
    }

    // === MÉTODOS DE ALERTA ===
    private void MostrarAvisoNaveReparada()
    {
        if (alertaNaveReparada != null)
        {
            alertaNaveReparada.MostrarAvisoNaveReparada(true);
            avisoTimer = 3f;
        }
    }

    private void MostrarAvisoFaltanMateriales()
    {
        if (alertaMateriales != null)
        {
            alertaMateriales.MostrarAvisoFaltanMateriales(true);
            avisoTimer = 3f;
        }
    }

    private void MostrarAvisoInteraccionNave()
    {
        if (alertaInteraccionNave != null)
        {
            alertaInteraccionNave.MostrarAvisodeInteraccionNave(true); // Usa el mismo método para mostrar
            avisoTimer = 3f; // Duración del aviso
        }
    }

    private void OcultarAvisoInteraccionNave()
    {
        if (alertaInteraccionNave != null)
        {
            alertaInteraccionNave.MostrarAvisodeInteraccionNave(false); // Usa el mismo método para mostrar
            avisoTimer = 3f; // Duración del aviso
        }
    }

    private void MostrarAvisoInteraccionNaveFinJuego()
    {
        if (alertaInteraccionNaveFinJuego != null)
        {
            alertaInteraccionNaveFinJuego.MostrarAvisodeInteraccionNaveFinJuego(true); // Usa el mismo método para mostrar
            avisoTimer = 3f; // Duración del aviso
        }
    }
}
