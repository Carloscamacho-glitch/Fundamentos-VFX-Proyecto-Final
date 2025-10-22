using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private GameObject naveRota; // Nave dañada
    [SerializeField] private GameObject nave;     // Nave reparada
    [SerializeField] private float rangoDeteccion; // Distancia para activar reparación
    private bool naverReparada = false;

    [SerializeField] private Transform jugador;

    [SerializeField] private AlertaMateriales alertaMateriales;
    [SerializeField] private AlertaNaveReparada alertaNaveReparada;
    [SerializeField] private float avisoTimer = 0f;

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
                if (player.TieneMotor() && player.GetMateriales() >= 2 && !naverReparada)
                {
                    RepararNave();
                    naverReparada = true;
                    MostrarAvisoNaveReparada();
                }
                else if (naverReparada)
                {
                    return;
                }
                else
                {
                    MostrarAvisoFaltanMateriales();
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

    private void RepararNave()
    {
        naveRota.SetActive(false);
        nave.SetActive(true);
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
