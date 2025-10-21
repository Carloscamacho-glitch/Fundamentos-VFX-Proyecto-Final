using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private GameObject naveRota; // Nave dañada
    [SerializeField] private GameObject nave;     // Nave reparada
    [SerializeField] private float rangoDeteccion; // Distancia para activar reparación
    private bool naverReparada = false;

    [SerializeField] private Transform jugador;

    void Update()
    {
        if (jugador == null) return;

        PlayerController player = jugador.GetComponent<PlayerController>();

        // Calcular distancia al jugador
        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Si está dentro del rango de detección
        if (distancia <= rangoDeteccion )
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (player.TieneMotor() && player.GetMateriales() >= 2 && !naverReparada)
                {
                    RepararNave();
                    naverReparada = true;
                }
                else if (naverReparada)
                {
                    return;
                }
                else
                {
                    Debug.Log("No tienes los materiales o el motor necesario para reparar la nave."); // poner en interfaz después
                }
            }else
            {
                Debug.Log("Presiona 'E' para reparar la nave.");    // poner en interfaz después
            }
        }
    }

    private void RepararNave()
    {
        Debug.Log("¡La nave ha sido reparada!"); // poner en interfaz después

        naveRota.SetActive(false);
        nave.SetActive(true);
    }
}
