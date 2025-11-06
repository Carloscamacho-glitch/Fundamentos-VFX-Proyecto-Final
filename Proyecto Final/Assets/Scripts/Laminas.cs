using UnityEngine;

public class Laminas : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private InventarioLaminas inventarioLaminas;

    private void Update()
    {
        // Rotar continuamente alrededor del eje Y local
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Accedemos al componente del jugador que gestiona materiales
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddMateriales(1);
            inventarioLaminas.SetLaminas();
        }

        // Destruimos el objeto tras recogerlo
        Destroy(gameObject);
    }
}
