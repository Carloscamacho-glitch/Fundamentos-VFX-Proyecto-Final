using UnityEngine;

public class NaveController : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private GameObject naveRota;               // la nave rota en el escenario
    [SerializeField] private GameObject nave;                   // la nave reparada que se activará

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.TieneMotor() && player.GetMateriales() >= 2)
        {
            if (naveRota != null) naveRota.SetActive(false);
            if (nave != null) nave.SetActive(true);

            Debug.Log("La nave ha sido reparada y activada correctamente.");
        }
        else
        {
            Debug.Log("No tienes los materiales o el motor necesarios para reparar la nave.");
        }
    }
}
